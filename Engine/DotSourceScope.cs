// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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

        private DotSourceScope(
            Dictionary<string, HashSet<string>> namesByFile,
            Dictionary<string, ParsedFile> parsedByFile)
        {
            this.namesByFile = namesByFile;
            this.parsedByFile = parsedByFile;
        }

        private struct ParsedFile
        {
            internal ScriptBlockAst Ast;
            internal Token[] Tokens;
            internal ParseError[] Errors;
        }

        internal IEnumerable<string> GetFunctionsInScope(string filePath)
        {
            if (filePath != null && namesByFile.TryGetValue(Normalize(filePath), out var names))
            {
                return names;
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Hands over the syntax tree this scope already produced for a file, so that the file is parsed
        /// once per run rather than once here and again when it is analyzed. The entry is dropped on the
        /// way out, which keeps the trees alive only until the files that own them have been analyzed.
        /// </summary>
        internal bool TryTakeParse(string filePath, out ScriptBlockAst ast, out Token[] tokens, out ParseError[] errors)
        {
            string normalized = filePath != null ? Normalize(filePath) : null;
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
            var known = new HashSet<string>(PathComparer);
            foreach (var path in filePaths)
            {
                known.Add(Normalize(path));
            }

            var definitions = new Dictionary<string, HashSet<string>>(PathComparer);
            var parsed = new Dictionary<string, ParsedFile>(PathComparer);
            var dotSourced = new Dictionary<string, List<string>>(PathComparer);

            foreach (var path in filePaths)
            {
                var normalized = Normalize(path);
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
                    if (!string.IsNullOrWhiteSpace(function.Name))
                    {
                        functions.Add(function.Name);
                    }
                }

                foreach (CommandAst command in ast.FindAll(node => node is CommandAst, true))
                {
                    var target = ResolveDotSourceTarget(command, path);
                    if (target != null && known.Contains(target))
                    {
                        if (!dotSourced.TryGetValue(normalized, out var targets))
                        {
                            targets = new List<string>();
                            dotSourced[normalized] = targets;
                        }
                        targets.Add(target);
                    }
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

            return new DotSourceScope(namesByFile, parsed);
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

        private static string ResolveDotSourceTarget(CommandAst command, string containingFile)
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
            // The only variables worth expanding are the ones that name the script's own location.
            raw = raw.Replace("$PSScriptRoot", directory).Replace("${PSScriptRoot}", directory);
            if (raw.IndexOf('$') >= 0)
            {
                return null;
            }

            try
            {
                return Normalize(Path.IsPathRooted(raw) ? raw : Path.Combine(directory, raw));
            }
            catch
            {
                return null;
            }
        }

        private static string Normalize(string path)
        {
            try
            {
                return CanonicalizeCase(Path.GetFullPath(path));
            }
            catch
            {
                return path;
            }
        }

        /// <summary>
        /// Resolves the file name segment of <paramref name="fullPath"/> to its actual on-disk casing,
        /// so that dictionary keys reflect what the real filesystem considers "the same file" rather than
        /// a guess about the platform's case sensitivity. An exact (ordinal) match against a directory
        /// entry always wins, so two files that a case-sensitive filesystem treats as distinct (such as
        /// 'Lib.ps1' and 'lib.ps1' both existing side by side) keep distinct keys. Only when the requested
        /// name has no exact match - e.g. a dot-source target typed with different case than the file it
        /// refers to - does this fall back to a case-insensitive match, folding it onto the real file's key.
        /// This is correct on any platform or mount, including a case-sensitive volume on an otherwise
        /// case-insensitive OS such as macOS, without assuming a single case-sensitivity rule per OS.
        /// </summary>
        private static string CanonicalizeCase(string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || !Directory.Exists(directory))
            {
                return fullPath;
            }

            string caseInsensitiveMatch = null;
            try
            {
                foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
                {
                    string entryName = Path.GetFileName(entry);
                    if (string.Equals(entryName, fileName, StringComparison.Ordinal))
                    {
                        return Path.Combine(directory, entryName);
                    }
                    if (caseInsensitiveMatch == null && string.Equals(entryName, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        caseInsensitiveMatch = entryName;
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return caseInsensitiveMatch != null ? Path.Combine(directory, caseInsensitiveMatch) : fullPath;
        }
    }
}
