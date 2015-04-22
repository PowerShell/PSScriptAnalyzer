using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Commands
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
        private ScriptBlockAst ast                         = null;
        private IEnumerable<IRule> rules                   = null;

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
                validationResults.Add("InvalidPaths",  new List<string>());
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

        /// <summary>
        /// Analyzes a single script file.
        /// </summary>
        /// <param name="filePath">The path to the file ot analyze</param>
        private void AnalyzeFile(string filePath)
        {
            Token[] tokens = null;
            ParseError[] errors = null;
            List<DiagnosticRecord> diagnostics = new List<DiagnosticRecord>();
            List<SuppressedRecord> suppressed = new List<SuppressedRecord>();

            // Use a List of KVP rather than dictionary, since for a script containing inline functions with same signature, keys clash
            List<KeyValuePair<CommandInfo, IScriptExtent>> cmdInfoTable = new List<KeyValuePair<CommandInfo, IScriptExtent>>();            

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

            Dictionary<string, List<RuleSuppression>> ruleSuppressions = Helper.Instance.GetRuleSuppression(ast);

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
                foreach (IScriptRule scriptRule in ScriptAnalyzer.Instance.ScriptRules)
                {
                    if ((includeRule == null || includeRule.Contains(scriptRule.GetName(), StringComparer.OrdinalIgnoreCase)) && 
                        (excludeRule == null || !excludeRule.Contains(scriptRule.GetName(), StringComparer.OrdinalIgnoreCase)))
                    {
                        WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, scriptRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            var records = Helper.Instance.SuppressRule(scriptRule.GetName(), ruleSuppressions, scriptRule.AnalyzeScript(ast, filePath).ToList());
                            diagnostics.AddRange(records.Item2);
                            suppressed.AddRange(records.Item1);
                        }
                        catch (Exception scriptRuleException)
                        {
                            WriteError(new ErrorRecord(scriptRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            continue;
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
                    if ((includeRule == null || includeRule.Contains(tokenRule.GetName(), StringComparer.OrdinalIgnoreCase)) && 
                        (excludeRule == null || !excludeRule.Contains(tokenRule.GetName(), StringComparer.OrdinalIgnoreCase)))
                    {
                        WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, tokenRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            var records = Helper.Instance.SuppressRule(tokenRule.GetName(), ruleSuppressions, tokenRule.AnalyzeTokens(tokens, filePath).ToList());
                            diagnostics.AddRange(records.Item2);
                            suppressed.AddRange(records.Item1);
                        }
                        catch (Exception tokenRuleException)
                        {
                            WriteError(new ErrorRecord(tokenRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, fileName));
                            continue;
                        } 
                    }
                }
            }

            #endregion

            #region DSC Resource Rules
            if (ScriptAnalyzer.Instance.DSCResourceRules != null)
            {
                // Run DSC Class rule
                foreach (IDSCResourceRule dscResourceRule in ScriptAnalyzer.Instance.DSCResourceRules)
                {
                    if ((includeRule == null || includeRule.Contains(dscResourceRule.GetName(), StringComparer.OrdinalIgnoreCase)) &&
                        (excludeRule == null || !excludeRule.Contains(dscResourceRule.GetName(), StringComparer.OrdinalIgnoreCase)))
                    {
                        WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            var records = Helper.Instance.SuppressRule(dscResourceRule.GetName(), ruleSuppressions, dscResourceRule.AnalyzeDSCClass(ast, filePath).ToList());
                            diagnostics.AddRange(records.Item2);
                            suppressed.AddRange(records.Item1);
                        }
                        catch (Exception dscResourceRuleException)
                        {
                            WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            continue;
                        }    
                    }
                }

                // Check if the supplied artifact is indeed part of the DSC resource
                // Step 1: Check if the artifact is under the "DSCResources" folder                
                DirectoryInfo dscResourceParent = Directory.GetParent(filePath);
                if (null != dscResourceParent)
                {
                    DirectoryInfo dscResourcesFolder = Directory.GetParent(dscResourceParent.ToString());
                    if (null != dscResourcesFolder)
                    {                        
                        if (String.Equals(dscResourcesFolder.Name, "dscresources",StringComparison.OrdinalIgnoreCase))
                        {
                            // Step 2: Ensure there is a Schema.mof in the same folder as the artifact
                            string schemaMofParentFolder = Directory.GetParent(filePath).ToString();
                            string[] schemaMofFile = Directory.GetFiles(schemaMofParentFolder, "*.schema.mof");

                            // Ensure Schema file exists and is the only one in the DSCResource folder
                            if (schemaMofFile != null && schemaMofFile.Count() == 1)
                            {
                                // Run DSC Rules only on module that matches the schema.mof file name without extension
                                if (schemaMofFile[0].Replace("schema.mof", "psm1") == filePath)
                                {
                                    // Run all DSC Rules
                                    foreach (IDSCResourceRule dscResourceRule in ScriptAnalyzer.Instance.DSCResourceRules)
                                    {
                                        if ((includeRule == null || includeRule.Contains(dscResourceRule.GetName(), StringComparer.OrdinalIgnoreCase)) &&
                                            (excludeRule == null || !excludeRule.Contains(dscResourceRule.GetName(), StringComparer.OrdinalIgnoreCase)))
                                        {
                                            WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                                            try
                                            {
                                                var records = Helper.Instance.SuppressRule(dscResourceRule.GetName(), ruleSuppressions, dscResourceRule.AnalyzeDSCResource(ast, filePath).ToList());
                                                diagnostics.AddRange(records.Item2);
                                                suppressed.AddRange(records.Item1);
                                            }
                                            catch (Exception dscResourceRuleException)
                                            {
                                                WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                                                continue;
                                            }  
                                        }
                                    }
                                }
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

                diagnostics.AddRange(ScriptAnalyzer.Instance.GetExternalRecord(ast, tokens, exRules.ToArray(), this, fileName));
            }

            #endregion

            if (severity != null)
            {
                var diagSeverity = severity.Select(item => Enum.Parse(typeof(DiagnosticSeverity), item));
                diagnostics = diagnostics.Where(item => diagSeverity.Contains(item.Severity)).ToList();
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
                    foreach (DiagnosticRecord diagnostic in diagnostics)
                    {
                        logger.LogObject(diagnostic, this);
                    }
                }
            }
        }

        #endregion
    }
}
