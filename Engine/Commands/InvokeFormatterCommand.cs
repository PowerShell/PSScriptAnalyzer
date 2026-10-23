// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    using PSSASettings = Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings;

    /// <summary>
    /// A cmdlet to format a PowerShell script text.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "Formatter", DefaultParameterSetName = ParameterSet_ScriptDefinition, SupportsShouldProcess = true)]
    [OutputType(typeof(string))]
    public class InvokeFormatterCommand : PSCmdlet, IOutputWriter
    {
        private const string ParameterSet_ScriptDefinition = nameof(ScriptDefinition);
        private const string ParameterSet_Path = nameof(Path);
        private const string IndentationRule = "PSUseConsistentIndentation";
        private const string defaultSettingsPreset = "CodeFormatting";
        private Settings inputSettings;
        private Range range;
        private bool settingsWereSpecified;

        /// <summary>
        /// The path to the file to format in place.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_Path, ValueFromPipelineByPropertyName = true, Position = 1)]
        [ValidateNotNull]
        [Alias("PSPath")]
        public string Path { get; set; }

        /// <summary>
        /// The script text to be formated.
        ///
        /// *NOTE*: Unlike ScriptBlock parameter, the ScriptDefinition parameter require a string value.
        /// </summary>
        [ParameterAttribute(Mandatory = true, ParameterSetName = ParameterSet_ScriptDefinition, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        [ValidateNotNull]
        public string ScriptDefinition { get; set; }

        /// <summary>
        /// A settings hashtable or a path to a PowerShell data file (.psd1) file that contains the settings.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 2)]
        [ValidateNotNull]
        public object Settings { get; set; } = defaultSettingsPreset;

        /// <summary>
        /// The range within which formatting should take place.
        ///
        /// The parameter is an array of integers of length 4 such that the first, second, third and last
        /// elements correspond to the start line number, start column number, end line number and
        /// end column number. These numbers must be greater than 0.
        /// </summary>
        /// <returns></returns>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 3)]
        [ValidateNotNull]
        [ValidateCount(4, 4)]
        public int[] Range { get; set; }

        /// <summary>
        /// The encoding to use when reading files without a byte-order mark.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Encoding { get; set; } = "utf-8";

#if DEBUG
        /// <summary>
        /// Attaches to an instance of a .Net debugger
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter AttachAndDebug
        {
            get { return attachAndDebug; }
            set { attachAndDebug = value; }
        }

        private bool attachAndDebug = false;
#endif

        protected override void BeginProcessing()
        {
#if DEBUG
            if (attachAndDebug)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                else
                {
                    System.Diagnostics.Debugger.Launch();
                }
            }
#endif

            this.range = Range == null ? null : new Range(Range[0], Range[1], Range[2], Range[3]);
            settingsWereSpecified = MyInvocation.BoundParameters.ContainsKey(nameof(Settings));
            try
            {
                inputSettings = PSSASettings.Create(Settings, this.MyInvocation.PSScriptRoot, this, GetResolvedProviderPathFromPSPath);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                        e,
                        "SETTINGS_ERROR",
                        ErrorCategory.InvalidData,
                        Settings));
            }

            if (inputSettings == null)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException(String.Format(
                        CultureInfo.CurrentCulture,
                        Strings.SettingsNotParsable)),
                    "SETTINGS_ERROR",
                    ErrorCategory.InvalidArgument,
                    Settings));
            }
        }

        protected override void ProcessRecord()
        {
            // todo add tests to check range formatting
            if (ParameterSetName == ParameterSet_Path)
            {
                ProviderInfo provider;
                var resolvedPaths = GetResolvedProviderPathFromPSPath(Path, out provider);
                if (!String.Equals(provider.Name, "FileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("Path must resolve to a file system path.", nameof(Path)),
                        "INVALID_PATH_PROVIDER",
                        ErrorCategory.InvalidArgument,
                        Path));
                }

                foreach (var path in resolvedPaths)
                {
                    FormatFile(path);
                }
            }
            else
            {
                string formattedScriptDefinition = Formatter.Format(ScriptDefinition, inputSettings, range, this);
                WriteObject(formattedScriptDefinition);
            }
        }

        private void FormatFile(string path)
        {
            Encoding encoding;
            string scriptDefinition;
            var requestedEncoding = System.Text.Encoding.GetEncoding(
                Encoding,
                EncoderFallback.ExceptionFallback,
                DecoderFallback.ExceptionFallback);
            using (var reader = new StreamReader(path, requestedEncoding, true))
            {
                scriptDefinition = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }

            bool inferIndentationKind = inputSettings.RuleArguments.TryGetValue(IndentationRule, out var indentationSettings)
                && (!settingsWereSpecified || !indentationSettings.ContainsKey("Kind"));
            if (inferIndentationKind)
            {
                indentationSettings["Kind"] = GetIndentationKind(scriptDefinition);
            }

            string formattedScriptDefinition;
            try
            {
                formattedScriptDefinition = Formatter.Format(scriptDefinition, inputSettings, range, this);
            }
            finally
            {
                if (inferIndentationKind && settingsWereSpecified)
                {
                    indentationSettings.Remove("Kind");
                }
            }
            if (!String.Equals(scriptDefinition, formattedScriptDefinition, StringComparison.Ordinal)
                && ShouldProcess(path, "Format file"))
            {
                File.WriteAllText(path, formattedScriptDefinition, encoding);
            }

            WriteObject(formattedScriptDefinition);
        }

        private static string GetIndentationKind(string scriptDefinition)
        {
            int tabIndentedLines = 0;
            int spaceIndentedLines = 0;
            string firstIndentationKind = "space";
            bool foundIndentation = false;
            string[] lines = scriptDefinition.Split('\n');
            Parser.ParseInput(scriptDefinition, out Token[] tokens, out ParseError[] errors);
            var countedLines = new HashSet<int>();

            foreach (var token in tokens)
            {
                if (token.Kind == TokenKind.Comment
                    || token.Kind == TokenKind.NewLine
                    || token.Kind == TokenKind.LineContinuation
                    || token.Kind == TokenKind.EndOfInput)
                {
                    continue;
                }

                int lineNumber = token.Extent.StartLineNumber;
                if (!countedLines.Add(lineNumber))
                {
                    continue;
                }

                string line = lines[lineNumber - 1].TrimEnd('\r');
                int indentationLength = token.Extent.StartColumnNumber - 1;
                if (indentationLength == 0 || indentationLength > line.Length)
                {
                    continue;
                }

                char indentationCharacter = line[0];
                bool isIndentedCode = true;
                for (int i = 0; i < indentationLength; i++)
                {
                    if (line[i] != ' ' && line[i] != '\t')
                    {
                        isIndentedCode = false;
                        break;
                    }
                }

                if (!isIndentedCode)
                {
                    continue;
                }

                if (!foundIndentation)
                {
                    firstIndentationKind = indentationCharacter == '\t' ? "tab" : "space";
                    foundIndentation = true;
                }

                if (indentationCharacter == '\t')
                {
                    tabIndentedLines++;
                }
                else
                {
                    spaceIndentedLines++;
                }
            }

            return tabIndentedLines > spaceIndentedLines
                ? "tab"
                : tabIndentedLines < spaceIndentedLines
                    ? "space"
                    : firstIndentationKind;
        }

        protected override void EndProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.EndProcessing();
        }

        protected override void StopProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.StopProcessing();
        }
    }
}
