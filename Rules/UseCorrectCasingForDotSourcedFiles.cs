// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseCorrectCasingForDotSourcedFiles: Check that a dot-sourced file's path matches the exact
    /// on-disk casing of the file it refers to.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseCorrectCasingForDotSourcedFiles : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of UseCorrectCasingForDotSourcedFiles type.
        /// </summary>
        public UseCorrectCasingForDotSourcedFiles()
        {
            Enable = false;
        }

        /// <summary>
        /// AnalyzeScript: Analyze the script to check that dot-sourced paths match the real,
        /// on-disk casing of the file they refer to.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast is null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Without a real file on disk to resolve relative paths and $PSScriptRoot against,
            // there is nothing to compare the literal's casing to.
            if (string.IsNullOrEmpty(fileName))
            {
                yield break;
            }

            string containingDirectory;
            try
            {
                containingDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            }
            catch
            {
                yield break;
            }

            IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);
            foreach (CommandAst commandAst in commandAsts)
            {
                if (commandAst.InvocationOperator != TokenKind.Dot || commandAst.CommandElements.Count == 0)
                {
                    continue;
                }

                Ast literalAst = commandAst.CommandElements[0];
                string raw;
                if (literalAst is StringConstantExpressionAst constant)
                {
                    raw = constant.Value;
                }
                else if (literalAst is ExpandableStringExpressionAst expandable)
                {
                    raw = expandable.Value;
                }
                else
                {
                    raw = null;
                }
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                // PowerShell folds '\' to '/' in FileSystem paths on Linux and macOS, where System.IO treats
                // a backslash as an ordinary file name character; resolve against the platform separator.
                string platformRaw = Path.DirectorySeparatorChar == '\\' ? raw : raw.Replace('\\', '/');

                // The only variable worth expanding is the one that names the script's own
                // location; anything else means the target cannot be resolved statically.
                string expanded = platformRaw.Replace("$PSScriptRoot", containingDirectory).Replace("${PSScriptRoot}", containingDirectory);
                if (expanded.IndexOf('$') >= 0)
                {
                    continue;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(Path.IsPathRooted(expanded) ? expanded : Path.Combine(containingDirectory, expanded));
                }
                catch
                {
                    continue;
                }

                if (!TryGetOnDiskCasing(fullPath, out string actualFileName) ||
                    string.Equals(actualFileName, Path.GetFileName(fullPath), StringComparison.Ordinal))
                {
                    continue;
                }

                // Only the file-name segment of the literal is corrected; reconstructing a
                // $PSScriptRoot-relative literal for arbitrary directory-segment mismatches is not
                // attempted, so this only reports (without a fix) when a parent directory's casing
                // also does not match what is on disk.
                string rawFileName = raw.Substring(raw.LastIndexOfAny(new[] { '/', '\\' }) + 1);
                string correctedLiteral = null;
                if (string.Equals(rawFileName, Path.GetFileName(fullPath), StringComparison.OrdinalIgnoreCase))
                {
                    correctedLiteral = ReplaceFinalFileName(literalAst.Extent.Text, raw, rawFileName, actualFileName);
                }

                var correctedFileName = actualFileName;
                IEnumerable<CorrectionExtent> corrections = Enumerable.Empty<CorrectionExtent>();
                if (correctedLiteral != null)
                {
                    corrections = new[]
                    {
                        new CorrectionExtent(
                            literalAst.Extent.StartLineNumber,
                            literalAst.Extent.EndLineNumber,
                            literalAst.Extent.StartColumnNumber,
                            literalAst.Extent.EndColumnNumber,
                            correctedLiteral,
                            literalAst.Extent.File,
                            GetDescription()),
                    };
                }

                yield return new DiagnosticRecord(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.UseCorrectCasingForDotSourcedFilesError,
                        raw,
                        correctedFileName),
                    literalAst.Extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName,
                    correctedFileName,
                    corrections);
            }
        }

        /// <summary>
        /// Resolves the real, on-disk file name for <paramref name="fullPath"/> by asking the
        /// filesystem, rather than guessing at a platform's case-sensitivity rule. Returns
        /// <see langword="false"/> when the file cannot be found at all (that is a different,
        /// unrelated problem - not this rule's concern).
        /// </summary>
        private static bool TryGetOnDiskCasing(string fullPath, out string actualFileName)
        {
            actualFileName = null;
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || !Directory.Exists(directory))
            {
                return false;
            }

            try
            {
                // An exact match always wins over case-insensitive candidates, which on a case-sensitive
                // file system are different files. Only a single unambiguous candidate is reported;
                // with several, any suggestion could change which script is loaded.
                string candidate = null;
                int candidates = 0;
                foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
                {
                    string entryName = Path.GetFileName(entry);
                    if (string.Equals(entryName, fileName, StringComparison.Ordinal))
                    {
                        actualFileName = entryName;
                        return true;
                    }
                    if (string.Equals(entryName, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        candidate = entryName;
                        candidates++;
                    }
                }

                if (candidates == 1)
                {
                    actualFileName = candidate;
                    return true;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return false;
        }

        /// <summary>
        /// Replaces only the final file name of the path inside the original source literal, keeping its
        /// quoting and prefix. Returns null when the literal's text does not contain the path verbatim
        /// (for example because of escape sequences), as the span cannot be identified safely then.
        /// </summary>
        private static string ReplaceFinalFileName(string literalText, string raw, string rawFileName, string actualFileName)
        {
            int pathStart = literalText.IndexOf(raw, StringComparison.Ordinal);
            if (pathStart < 0)
            {
                return null;
            }

            int nameStart = pathStart + raw.Length - rawFileName.Length;
            return literalText.Substring(0, nameStart) + actualFileName + literalText.Substring(nameStart + rawFileName.Length);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseCorrectCasingForDotSourcedFilesName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingForDotSourcedFilesCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingForDotSourcedFilesDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
