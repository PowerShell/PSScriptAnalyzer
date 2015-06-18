//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Text.RegularExpressions;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.IO;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// InvokeScriptAnalyzerCommand: Cmdlet to statically check PowerShell scripts.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptAnalyzer", HelpUri = "http://go.microsoft.com/fwlink/?LinkId=525914")]
    public class InvokeScriptAnalyzerCommand : PSCmdlet
    {
        #region Parameters
        /// <summary>
        /// Path: The path to the file or folder to invoke PSScriptAnalyzer on.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNull]
        [Alias("PSPath")]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary>
        /// CustomRulePath: The path to the file containing custom rules to run.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] CustomizedRulePath
        {
            get { return customizedRulePath; }
            set { customizedRulePath = value; }
        }
        private string[] customizedRulePath;

        /// <summary>
        /// ExcludeRule: Array of names of rules to be disabled.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] ExcludeRule
        {
            get { return excludeRule; }
            set { excludeRule = value; }
        }
        private string[] excludeRule;

        /// <summary>
        /// IncludeRule: Array of names of rules to be enabled.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] IncludeRule
        {
            get { return includeRule; }
            set { includeRule = value; }
        }
        private string[] includeRule;

        /// <summary>
        /// IncludeRule: Array of the severity types to be enabled.
        /// </summary>
        [ValidateSet("Warning", "Error", "Information", IgnoreCase = true)]
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        private string[] severity;

        /// <summary>
        /// Recurse: Apply to all files within subfolders under the path
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }
        private bool recurse;

        /// <summary>
        /// ShowSuppressed: Show the suppressed message
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter SuppressedOnly
        {
            get { return suppressedOnly; }
            set { suppressedOnly = value; }
        }
        private bool suppressedOnly;

        #endregion Parameters

        #region Private Members

        Dictionary<string, List<string>> validationResults = new Dictionary<string, List<string>>();
        private ScriptBlockAst ast = null;
        private IEnumerable<IRule> rules = null;

        #endregion

        #region Ovserrides

        /// <summary>
        /// Imports all known rules and loggers.
        /// </summary>
        protected override void BeginProcessing()
        {
            #region Set PSCmdlet property of Helper

            Helper.Instance.MyCmdlet = this;

            #endregion

            #region Verifies rule extensions and loggers path

            List<string> paths = new List<string>();

            if (customizedRulePath != null) paths.AddRange(customizedRulePath.ToList());

            if (paths.Count > 0)
            {
                validationResults = ScriptAnalyzer.Instance.CheckRuleExtension(paths.ToArray(), this);
                foreach (string extension in validationResults["InvalidPaths"])
                {
                    WriteWarning(string.Format(CultureInfo.CurrentCulture, Strings.MissingRuleExtension, extension));
                }
            }
            else
            {
                validationResults.Add("InvalidPaths", new List<string>());
                validationResults.Add("ValidModPaths", new List<string>());
                validationResults.Add("ValidDllPaths", new List<string>());
            }

            #endregion

            #region Initializes Rules

            try
            {
                if (validationResults["ValidDllPaths"].Count == 0 &&
                    validationResults["ValidModPaths"].Count == 0)
                {
                    ScriptAnalyzer.Instance.Initialize();
                }
                else
                {
                    ScriptAnalyzer.Instance.Initilaize(validationResults);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, ex.HResult.ToString("X", CultureInfo.CurrentCulture),
                    ErrorCategory.NotSpecified, this));
            }

            #endregion

            #region Verify rules

            rules = ScriptAnalyzer.Instance.ScriptRules.Union<IRule>(
                    ScriptAnalyzer.Instance.TokenRules).Union<IRule>(
                    ScriptAnalyzer.Instance.ExternalRules ?? Enumerable.Empty<IExternalRule>());

            if (rules == null || rules.Count() == 0)
            {
                ThrowTerminatingError(new ErrorRecord(new Exception(), string.Format(CultureInfo.CurrentCulture, Strings.RulesNotFound), ErrorCategory.ResourceExists, this));
            }

            #endregion
        }

        /// <summary>
        /// Analyzes the given script/directory.
        /// </summary>
        protected override void ProcessRecord()
        {
            // throws Item Not Found Exception
            path = this.SessionState.Path.GetResolvedPSPathFromPSPath(path).First().ToString();
            ProcessPath(path);
        }

        #endregion

        #region Methods

        private void ProcessPath(string path)
        {
            const string ps1Suffix = "ps1";
            const string psm1Suffix = "psm1";
            const string psd1Suffix = "psd1";

            if (path == null)
            {
                ThrowTerminatingError(new ErrorRecord(new FileNotFoundException(),
                    string.Format(CultureInfo.CurrentCulture, Strings.FileNotFound, path),
                    ErrorCategory.InvalidArgument, this));
            }

            if (Directory.Exists(path))
            {
                if (recurse)
                {

                    foreach (string filePath in Directory.GetFiles(path))
                    {
                        ProcessPath(filePath);
                    }
                    foreach (string filePath in Directory.GetDirectories(path))
                    {
                        ProcessPath(filePath);
                    }
                }
                else
                {
                    foreach (string filePath in Directory.GetFiles(path))
                    {
                        ProcessPath(filePath);
                    }
                }
            }
            else if (File.Exists(path))
            {
                WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseFileMessage, path));
                if ((path.Length > ps1Suffix.Length && path.Substring(path.Length - ps1Suffix.Length).Equals(ps1Suffix, StringComparison.OrdinalIgnoreCase)) ||
                    (path.Length > psm1Suffix.Length && path.Substring(path.Length - psm1Suffix.Length).Equals(psm1Suffix, StringComparison.OrdinalIgnoreCase)) ||
                    (path.Length > psd1Suffix.Length && path.Substring(path.Length - psd1Suffix.Length).Equals(psd1Suffix, StringComparison.OrdinalIgnoreCase)))
                {
                    AnalyzeFile(path);
                }
            }
            else
            {
                WriteError(new ErrorRecord(new FileNotFoundException(), string.Format(CultureInfo.CurrentCulture, Strings.FileNotFound, path), ErrorCategory.InvalidArgument, this));
            }

        }

        ConcurrentBag<DiagnosticRecord> diagnostics;
        ConcurrentBag<SuppressedRecord> suppressed;
        Dictionary<string, List<RuleSuppression>> ruleSuppressions;
        List<Regex> includeRegexList;
        List<Regex> excludeRegexList;
        ConcurrentDictionary<string, List<object>> ruleDictionary;

        /// <summary>
        /// Analyzes a single script file.
        /// </summary>
        /// <param name="filePath">The path to the file ot analyze</param>
        private void AnalyzeFile(string filePath)
        {
            Token[] tokens = null;
            ParseError[] errors = null;
            ConcurrentBag<DiagnosticRecord> diagnostics = new ConcurrentBag<DiagnosticRecord>();
            ConcurrentBag<SuppressedRecord> suppressed = new ConcurrentBag<SuppressedRecord>();
            BlockingCollection<List<object>> verboseOrErrors = new BlockingCollection<List<object>>();

            // Use a List of KVP rather than dictionary, since for a script containing inline functions with same signature, keys clash
            List<KeyValuePair<CommandInfo, IScriptExtent>> cmdInfoTable = new List<KeyValuePair<CommandInfo, IScriptExtent>>();

            //Check wild card input for the Include/ExcludeRules and create regex match patterns
            includeRegexList = new List<Regex>();
            excludeRegexList = new List<Regex>();
            if (includeRule != null)
            {
                foreach (string rule in includeRule)
                {
                    Regex includeRegex = new Regex(String.Format("^{0}$", Regex.Escape(rule).Replace(@"\*", ".*")), RegexOptions.IgnoreCase);
                    includeRegexList.Add(includeRegex);
                }
            }
            if (excludeRule != null)
            {
                foreach (string rule in excludeRule)
                {
                    Regex excludeRegex = new Regex(String.Format("^{0}$", Regex.Escape(rule).Replace(@"\*", ".*")), RegexOptions.IgnoreCase);
                    excludeRegexList.Add(excludeRegex);
                }
            }


            //Parse the file
            if (File.Exists(filePath))
            {
                ast = Parser.ParseFile(filePath, out tokens, out errors);
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new FileNotFoundException(),
                    string.Format(CultureInfo.CurrentCulture, Strings.InvalidPath, filePath),
                    ErrorCategory.InvalidArgument, filePath));
            }

            if (errors != null && errors.Length > 0)
            {
                foreach (ParseError error in errors)
                {
                    string parseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParserErrorFormat, error.Extent.File, error.Message.TrimEnd('.'), error.Extent.StartLineNumber, error.Extent.StartColumnNumber);
                    WriteError(new ErrorRecord(new ParseException(parseErrorMessage), parseErrorMessage, ErrorCategory.ParserError, error.ErrorId));
                }
            }

            if (errors.Length > 10)
            {
                string manyParseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParserErrorMessage, System.IO.Path.GetFileName(filePath));
                WriteError(new ErrorRecord(new ParseException(manyParseErrorMessage), manyParseErrorMessage, ErrorCategory.ParserError, filePath));

                return;
            }

            ruleSuppressions = Helper.Instance.GetRuleSuppression(ast);

            foreach (List<RuleSuppression> ruleSuppressionsList in ruleSuppressions.Values)
            {
                foreach (RuleSuppression ruleSuppression in ruleSuppressionsList)
                {
                    if (!String.IsNullOrWhiteSpace(ruleSuppression.Error))
                    {
                        WriteError(new ErrorRecord(new ArgumentException(ruleSuppression.Error), ruleSuppression.Error, ErrorCategory.InvalidArgument, ruleSuppression));
                    }
                }
            }

            #region Run VariableAnalysis
            try
            {
                Helper.Instance.InitializeVariableAnalysis(ast);
            }
            catch { }
            #endregion

            Helper.Instance.Tokens = tokens;

            #region Run ScriptRules
            //Trim down to the leaf element of the filePath and pass it to Diagnostic Record
            string fileName = System.IO.Path.GetFileName(filePath);

            if (ScriptAnalyzer.Instance.ScriptRules != null)
            {
                var tasks = ScriptAnalyzer.Instance.ScriptRules.Select(scriptRule => Task.Factory.StartNew(() =>
                    {
                        bool includeRegexMatch = false;
                        bool excludeRegexMatch = false;

                        foreach (Regex include in includeRegexList)
                        {
                            if (include.IsMatch(scriptRule.GetName()))
                            {
                                includeRegexMatch = true;
                                break;
                            }
                        }

                        foreach (Regex exclude in excludeRegexList)
                        {
                            if (exclude.IsMatch(scriptRule.GetName()))
                            {
                                excludeRegexMatch = true;
                                break;
                            }
                        }

                        if ((includeRule == null || includeRegexMatch) && (excludeRule == null || !excludeRegexMatch))
                        {
                            List<object> result = new List<object>();

                            result.Add(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, scriptRule.GetName()));

                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                            try
                            {
                                var records = Helper.Instance.SuppressRule(scriptRule.GetName(), ruleSuppressions, scriptRule.AnalyzeScript(ast, ast.Extent.File).ToList());
                                foreach (var record in records.Item2)
                                {
                                    diagnostics.Add(record);
                                }
                                foreach (var suppressedRec in records.Item1)
                                {
                                    suppressed.Add(suppressedRec);
                                }
                            }
                            catch (Exception scriptRuleException)
                            {
                                result.Add(new ErrorRecord(scriptRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, ast.Extent.File));
                            }

                            verboseOrErrors.Add(result);
                        }
                }));

                Task.Factory.ContinueWhenAll(tasks.ToArray(), t => verboseOrErrors.CompleteAdding());

                while (!verboseOrErrors.IsCompleted)
                {
                    List<object> data = null;
                    try
                    {
                        data = verboseOrErrors.Take();
                    }
                    catch (InvalidOperationException) { }

                    if (data != null)
                    {
                        WriteVerbose(data[0] as string);
                        if (data.Count == 2)
                        {
                            WriteError(data[1] as ErrorRecord);
                        }
                    }
                }
            }

            #endregion

            #region Run Token Rules

            if (ScriptAnalyzer.Instance.TokenRules != null)
            {
                foreach (ITokenRule tokenRule in ScriptAnalyzer.Instance.TokenRules)
                {
                    bool includeRegexMatch = false;
                    bool excludeRegexMatch = false;
                    foreach (Regex include in includeRegexList)
                    {
                        if (include.IsMatch(tokenRule.GetName()))
                        {
                            includeRegexMatch = true;
                            break;
                        }
                    }
                    foreach (Regex exclude in excludeRegexList)
                    {
                        if (exclude.IsMatch(tokenRule.GetName()))
                        {
                            excludeRegexMatch = true;
                            break;
                        }
                    }
                    if ((includeRule == null || includeRegexMatch) && (excludeRule == null || !excludeRegexMatch))
                    {
                        WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, tokenRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            var records = Helper.Instance.SuppressRule(tokenRule.GetName(), ruleSuppressions, tokenRule.AnalyzeTokens(tokens, filePath).ToList());
                            foreach (var record in records.Item2)
                            {
                                diagnostics.Add(record);
                            }
                            foreach (var suppressedRec in records.Item1)
                            {
                                suppressed.Add(suppressedRec);
                            }
                        }
                        catch (Exception tokenRuleException)
                        {
                            WriteError(new ErrorRecord(tokenRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, fileName));
                        }
                    }
                }
            }

            #endregion

            #region DSC Resource Rules
            if (ScriptAnalyzer.Instance.DSCResourceRules != null)
            {
                // Invoke AnalyzeDSCClass only if the ast is a class based resource
                if (Helper.Instance.IsDscResourceClassBased(ast))
                {
                    // Run DSC Class rule
                    foreach (IDSCResourceRule dscResourceRule in ScriptAnalyzer.Instance.DSCResourceRules)
                    {
                        bool includeRegexMatch = false;
                        bool excludeRegexMatch = false;

                        foreach (Regex include in includeRegexList)
                        {
                            if (include.IsMatch(dscResourceRule.GetName()))
                            {
                                includeRegexMatch = true;
                                break;
                            }
                        }

                        foreach (Regex exclude in excludeRegexList)
                        {
                            if (exclude.IsMatch(dscResourceRule.GetName()))
                            {
                                excludeRegexMatch = true;
                                break;
                            }
                        }

                        if ((includeRule == null || includeRegexMatch) && (excludeRule == null || excludeRegexMatch))
                        {
                            WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                            try
                            {
                                var records = Helper.Instance.SuppressRule(dscResourceRule.GetName(), ruleSuppressions, dscResourceRule.AnalyzeDSCClass(ast, filePath).ToList());
                                foreach (var record in records.Item2)
                                {
                                    diagnostics.Add(record);
                                }
                                foreach (var suppressedRec in records.Item1)
                                {
                                    suppressed.Add(suppressedRec);
                                }
                            }
                            catch (Exception dscResourceRuleException)
                            {
                                WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            }
                        }
                    }
                }

                // Check if the supplied artifact is indeed part of the DSC resource
                if (Helper.Instance.IsDscResourceModule(filePath))
                {
                    // Run all DSC Rules
                    foreach (IDSCResourceRule dscResourceRule in ScriptAnalyzer.Instance.DSCResourceRules)
                    {
                        bool includeRegexMatch = false;
                        bool excludeRegexMatch = false;
                        foreach (Regex include in includeRegexList)
                        {
                            if (include.IsMatch(dscResourceRule.GetName()))
                            {
                                includeRegexMatch = true;
                                break;
                            }
                        }
                        foreach (Regex exclude in excludeRegexList)
                        {
                            if (exclude.IsMatch(dscResourceRule.GetName()))
                            {
                                excludeRegexMatch = true;
                            }
                        }
                        if ((includeRule == null || includeRegexMatch) && (excludeRule == null || !excludeRegexMatch))
                        {
                            WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                            try
                            {
                                var records = Helper.Instance.SuppressRule(dscResourceRule.GetName(), ruleSuppressions, dscResourceRule.AnalyzeDSCResource(ast, filePath).ToList());
                                foreach (var record in records.Item2)
                                {
                                    diagnostics.Add(record);
                                }
                                foreach (var suppressedRec in records.Item1)
                                {
                                    suppressed.Add(suppressedRec);
                                }
                            }
                            catch (Exception dscResourceRuleException)
                            {
                                WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            }
                        }
                    }

                }
            }
            #endregion

            #region Run External Rules

            if (ScriptAnalyzer.Instance.ExternalRules != null)
            {
                List<ExternalRule> exRules = new List<ExternalRule>();

                foreach (ExternalRule exRule in ScriptAnalyzer.Instance.ExternalRules)
                {
                    if ((includeRule == null || includeRule.Contains(exRule.GetName(), StringComparer.OrdinalIgnoreCase)) &&
                        (excludeRule == null || !excludeRule.Contains(exRule.GetName(), StringComparer.OrdinalIgnoreCase)))
                    {
                        string ruleName = string.Format(CultureInfo.CurrentCulture, "{0}\\{1}", exRule.GetSourceName(), exRule.GetName());
                        WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, ruleName));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            exRules.Add(exRule);
                        }
                        catch (Exception externalRuleException)
                        {
                            WriteError(new ErrorRecord(externalRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, fileName));
                        }
                    }
                }

                foreach (var record in ScriptAnalyzer.Instance.GetExternalRecord(ast, tokens, exRules.ToArray(), this, fileName))
                {
                    diagnostics.Add(record);
                }
            }

            #endregion

            IEnumerable<DiagnosticRecord> diagnosticsList = diagnostics;

            if (severity != null)
            {
                var diagSeverity = severity.Select(item => Enum.Parse(typeof(DiagnosticSeverity), item, true));
                diagnosticsList = diagnostics.Where(item => diagSeverity.Contains(item.Severity));
            }

            //Output through loggers
            foreach (ILogger logger in ScriptAnalyzer.Instance.Loggers)
            {
                if (SuppressedOnly)
                {
                    foreach (DiagnosticRecord suppressRecord in suppressed)
                    {
                        logger.LogObject(suppressRecord, this);
                    }
                }
                else
                {
                    foreach (DiagnosticRecord diagnostic in diagnosticsList)
                    {
                        logger.LogObject(diagnostic, this);
                    }
                }
            }
        }

        #endregion
    }
}