// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Groups the files of one analysis run by dot-source relationship and reports, per file, the
    /// functions that are in scope for it.
    /// </summary>
    /// <remarks>
    /// Files that a common root dot-sources share a session state at run time, so a call from one to a
    /// function defined in another binds locally. The relationship is not visible from the calling file:
    /// a module's lib files call each other without dot-sourcing, and are only in scope together because
    /// the .psm1 sourced them all. Membership is therefore resolved over the whole run.
    /// </remarks>
    internal sealed class DotSourceScope
    {
        // Keys are canonicalized against the real filesystem in Normalize(), so ordinal comparison is
        // correct on every platform and mount: it neither assumes Windows-style case-insensitivity nor
        // guesses at macOS's default (case-insensitive but case-preserving, and sometimes configured
        // case-sensitive) or Linux's (case-sensitive, but not always - e.g. mounted exFAT/NTFS volumes).
        private static readonly StringComparer PathComparer = StringComparer.Ordinal;

        private readonly Dictionary<string, HashSet<string>> namesByFile;
        private readonly Dictionary<string, ParsedFile> parsedByFile;
        private readonly Dictionary<string, List<LocalFunctionScope.ChildScopeNames>> childScopesByFile;
        private readonly PathCanonicalizer paths;

        private DotSourceScope(
            Dictionary<string, HashSet<string>> namesByFile,
            Dictionary<string, ParsedFile> parsedByFile,
            Dictionary<string, List<LocalFunctionScope.ChildScopeNames>> childScopesByFile,
            PathCanonicalizer paths)
        {
            this.namesByFile = namesByFile;
            this.parsedByFile = parsedByFile;
            this.childScopesByFile = childScopesByFile;
            this.paths = paths;
        }

        private struct ParsedFile
        {
            internal ScriptBlockAst Ast;
            internal Token[] Tokens;
            internal ParseError[] Errors;
        }

        internal IEnumerable<string> GetFunctionsInScope(string filePath)
        {
            if (filePath != null && namesByFile.TryGetValue(paths.Normalize(filePath), out var names))
            {
                return names;
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Functions that a dot-source inside a function or other child scope makes visible, but only to
        /// that scope: loading a file there does not export its definitions to the rest of the file.
        /// </summary>
        internal IEnumerable<LocalFunctionScope.ChildScopeNames> GetChildScopeFunctions(string filePath)
        {
            if (filePath != null && childScopesByFile.TryGetValue(paths.Normalize(filePath), out var scopes))
            {
                return scopes;
            }
            return null;
        }

        /// <summary>
        /// Hands over the syntax tree this scope already produced for a file, so that the file is parsed
        /// once per run rather than once here and again when it is analyzed. The entry is dropped on the
        /// way out, which keeps the trees alive only until the files that own them have been analyzed.
        /// </summary>
        internal bool TryTakeParse(string filePath, out ScriptBlockAst ast, out Token[] tokens, out ParseError[] errors)
        {
            string normalized = filePath != null ? paths.Normalize(filePath) : null;
            if (normalized != null && parsedByFile.TryGetValue(normalized, out var parsed))
            {
                parsedByFile.Remove(normalized);
                ast = parsed.Ast;
                tokens = parsed.Tokens;
                errors = parsed.Errors;
                return ast != null;
            }

            ast = null;
            tokens = null;
            errors = null;
            return false;
        }

        /// <param name="retainParses">
        /// Whether to keep the syntax trees for <see cref="TryTakeParse"/> to hand back. Only sound when the
        /// files cannot change between this pre-pass and the analysis; -Fix rewrites them, so it opts out.
        /// </param>
        internal static DotSourceScope Build(IReadOnlyList<string> filePaths, bool retainParses = true)
        {
            var paths = new PathCanonicalizer();
            var known = new HashSet<string>(PathComparer);
            foreach (var path in filePaths)
            {
                known.Add(paths.Normalize(path));
            }

            var definitions = new Dictionary<string, HashSet<string>>(PathComparer);
            var parsed = new Dictionary<string, ParsedFile>(PathComparer);
            var dotSourced = new Dictionary<string, List<string>>(PathComparer);
            var childDotSourced = new Dictionary<string, List<KeyValuePair<int, string>>>(PathComparer);

            foreach (var path in filePaths)
            {
                var normalized = paths.Normalize(path);
                var functions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                definitions[normalized] = functions;

                ScriptBlockAst ast;
                Token[] tokens;
                ParseError[] errors;
                try
                {
                    ast = Parser.ParseFile(path, out tokens, out errors);
                }
                catch
                {
                    continue;
                }
                if (ast == null)
                {
                    continue;
                }

                if (retainParses)
                {
                    parsed[normalized] = new ParsedFile { Ast = ast, Tokens = tokens, Errors = errors };
                }

                foreach (FunctionDefinitionAst function in ast.FindAll(node => node is FunctionDefinitionAst, true))
                {
                    if (!string.IsNullOrWhiteSpace(function.Name) && LocalFunctionScope.IsVisibleAcrossFiles(function))
                    {
                        functions.Add(function.Name);
                    }
                }

                foreach (CommandAst command in ast.FindAll(node => node is CommandAst, true))
                {
                    var target = ResolveDotSourceTarget(command, path, paths);
                    if (target == null || !known.Contains(target))
                    {
                        continue;
                    }

                    // Dot-sourcing inside a function or other child scope loads the file into that scope,
                    // so its definitions must not become visible to the rest of the file or its group.
                    var owningScope = LocalFunctionScope.GetOwningScope(command);
                    if (owningScope != null)
                    {
                        int ordinal = LocalFunctionScope.GetScopeOrdinal(ast, owningScope);
                        if (ordinal >= 0)
                        {
                            if (!childDotSourced.TryGetValue(normalized, out var childTargets))
                            {
                                childTargets = new List<KeyValuePair<int, string>>();
                                childDotSourced[normalized] = childTargets;
                            }
                            childTargets.Add(new KeyValuePair<int, string>(ordinal, target));
                        }
                        continue;
                    }

                    if (!dotSourced.TryGetValue(normalized, out var targets))
                    {
                        targets = new List<string>();
                        dotSourced[normalized] = targets;
                    }
                    targets.Add(target);
                }
            }

            // A file sees the functions of everything it dot-sources, transitively, and also those of the
            // files an ancestor sourced alongside it: at run time they all land in that ancestor's session
            // state. So a file's scope is the union of the closures of the entry points that reach it.
            // Merging symmetrically instead would be coarser than the language: a test script that sources
            // one library would be credited with every other library that shares a root.
            var namesByFile = new Dictionary<string, HashSet<string>>(PathComparer);
            foreach (var file in definitions.Keys)
            {
                namesByFile[file] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            foreach (var entryPoint in definitions.Keys)
            {
                var closure = Closure(entryPoint, dotSourced);
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var member in closure)
                {
                    if (definitions.TryGetValue(member, out var defined))
                    {
                        names.UnionWith(defined);
                    }
                }

                foreach (var member in closure)
                {
                    if (namesByFile.TryGetValue(member, out var scope))
                    {
                        scope.UnionWith(names);
                    }
                }
            }

            var childScopesByFile = new Dictionary<string, List<LocalFunctionScope.ChildScopeNames>>(PathComparer);
            foreach (var entry in childDotSourced)
            {
                var scopes = new List<LocalFunctionScope.ChildScopeNames>();
                foreach (var edge in entry.Value)
                {
                    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var member in Closure(edge.Value, dotSourced))
                    {
                        if (definitions.TryGetValue(member, out var defined))
                        {
                            names.UnionWith(defined);
                        }
                    }
                    scopes.Add(new LocalFunctionScope.ChildScopeNames { ContainerOrdinal = edge.Key, Names = names });
                }
                childScopesByFile[entry.Key] = scopes;
            }

            return new DotSourceScope(namesByFile, parsed, childScopesByFile, paths);
        }

        /// <summary>
        /// Returns <paramref name="start"/> together with everything it dot-sources, directly or indirectly.
        /// </summary>
        private static List<string> Closure(string start, Dictionary<string, List<string>> dotSourced)
        {
            var reached = new List<string> { start };
            var visited = new HashSet<string>(PathComparer) { start };
            var pending = new Stack<string>();
            pending.Push(start);

            while (pending.Count > 0)
            {
                if (!dotSourced.TryGetValue(pending.Pop(), out var targets))
                {
                    continue;
                }

                foreach (var target in targets)
                {
                    // The visited set also stops a dot-source cycle from looping forever.
                    if (visited.Add(target))
                    {
                        reached.Add(target);
                        pending.Push(target);
                    }
                }
            }

            return reached;
        }

        private static string ResolveDotSourceTarget(CommandAst command, string containingFile, PathCanonicalizer paths)
        {
            if (command.InvocationOperator != TokenKind.Dot || command.CommandElements.Count == 0)
            {
                return null;
            }

            string raw = null;
            switch (command.CommandElements[0])
            {
                case StringConstantExpressionAst constant: raw = constant.Value; break;
                case ExpandableStringExpressionAst expandable: raw = expandable.Value; break;
            }
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(Path.GetFullPath(containingFile));
            // PowerShell folds '\' to '/' in FileSystem paths on Linux and macOS, where System.IO does not.
            raw = PathCanonicalizer.ToPlatformSeparators(raw);
            // The only variables worth expanding are the ones that name the script's own location.
            raw = raw.Replace("$PSScriptRoot", directory).Replace("${PSScriptRoot}", directory);
            if (raw.IndexOf('$') >= 0)
            {
                return null;
            }

            try
            {
                return paths.Normalize(Path.IsPathRooted(raw) ? raw : Path.Combine(directory, raw));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Canonicalizes paths against the real file system and remembers directory listings and results,
        /// so a run enumerates each directory once rather than once per path it normalizes.
        /// Case rules are taken from the file system itself, never from the OS: an exact-case entry always
        /// wins, and only without one does the first case-insensitive entry apply (which on a
        /// case-insensitive volume is the same file). A mismatched reference is thus folded onto the real
        /// file, and the mismatch is left to PSUseCorrectCasingForDotSourcedFiles.
        /// </summary>
        private sealed class PathCanonicalizer
        {
            private sealed class DirectoryListing
            {
                internal readonly HashSet<string> Exact = new HashSet<string>(StringComparer.Ordinal);
                internal readonly Dictionary<string, string> FirstIgnoringCase =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            private readonly ConcurrentDictionary<string, DirectoryListing> listings =
                new ConcurrentDictionary<string, DirectoryListing>(StringComparer.Ordinal);
            private readonly ConcurrentDictionary<string, string> canonical =
                new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            internal static string ToPlatformSeparators(string path) =>
                Path.DirectorySeparatorChar == '\\' ? path : path.Replace('\\', '/');

            internal string Normalize(string path)
            {
                if (canonical.TryGetValue(path, out var cached))
                {
                    return cached;
                }

                string result;
                try
                {
                    result = CanonicalizeCase(Path.GetFullPath(ToPlatformSeparators(path)));
                }
                catch
                {
                    result = path;
                }
                canonical[path] = result;
                return result;
            }

            /// <summary>
            /// Resolves the file name segment of <paramref name="fullPath"/> to its actual on-disk casing,
            /// so that dictionary keys reflect what the real filesystem considers "the same file" rather than
            /// a guess about the platform's case sensitivity. An exact (ordinal) match against a directory
            /// entry always wins, so two files that a case-sensitive filesystem treats as distinct (such as
            /// 'Lib.ps1' and 'lib.ps1' both existing side by side) keep distinct keys. Only when the requested
            /// name has no exact match - e.g. a dot-source target typed with different case than the file it
            /// refers to - does this fall back to a case-insensitive match, folding it onto the real file's key.
            /// A mismatched reference is deliberately treated as the file it names: whether it would load
            /// depends on the file system, so UseCorrectCasingForDotSourcedFiles reports it instead.
            /// </summary>
            private string CanonicalizeCase(string fullPath)
            {
                string directory = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);
                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                {
                    return fullPath;
                }

                var listing = listings.GetOrAdd(directory, ListDirectory);
                if (listing == null || listing.Exact.Contains(fileName))
                {
                    return fullPath;
                }

                return listing.FirstIgnoringCase.TryGetValue(fileName, out var match)
                    ? Path.Combine(directory, match)
                    : fullPath;
            }

            private static DirectoryListing ListDirectory(string directory)
            {
                if (!Directory.Exists(directory))
                {
                    return null;
                }

                var listing = new DirectoryListing();
                try
                {
                    foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
                    {
                        string entryName = Path.GetFileName(entry);
                        listing.Exact.Add(entryName);
                        if (!listing.FirstIgnoringCase.ContainsKey(entryName))
                        {
                            listing.FirstIgnoringCase[entryName] = entryName;
                        }
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                return listing;
            }
        }
    }
}
