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
using Microsoft.Windows.Powershell.ScriptAnalyzer.Commands;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Threading;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer
{
    internal class ScriptAnalyzer
    {
        #region Private memebers

        private CompositionContainer container;

        #endregion

        #region Singleton
        private static object syncRoot = new Object();

        private static ScriptAnalyzer instance;

        public static ScriptAnalyzer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ScriptAnalyzer();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        // Initializes via ImportMany
        [ImportMany]
        public IEnumerable<IScriptRule> ScriptRules { get; private set; }

        [ImportMany]
        public IEnumerable<ITokenRule> TokenRules { get; private set; }

        [ImportMany]
        public IEnumerable<ILogger> Loggers { get; private set; }

        [ImportMany]
        public IEnumerable<IDSCResourceRule> DSCResourceRules { get; private set; }

        public List<ExternalRule> ExternalRules { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize : Initializes default rules, loggers and helper.
        /// </summary>
        public void Initialize()
        {
            // Clear external rules for each invoke.
            ExternalRules = new List<ExternalRule>();

            // Initialize helper
            Helper.Instance.Initialize();

            // An aggregate catalog that combines multiple catalogs.
            using (AggregateCatalog catalog = new AggregateCatalog())
            {
                // Adds all the parts found in the same directory.
                string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Assembly.GetExecutingAssembly().Location
                catalog.Catalogs.Add(new DirectoryCatalog(dirName));

                // Create the CompositionContainer with the parts in the catalog.
                container = new CompositionContainer(catalog);

                // Fill the imports of this object.
                try
                {
                    container.ComposeParts(this);
                }
                catch (CompositionException compositionException)
                {
                    Console.WriteLine(compositionException.ToString());
                }
            }
        }

        /// <summary>
        /// Initilaize : Initializes default rules, external rules and loggers.
        /// </summary>
        /// <param name="result">Path validation result.</param>
        public void Initilaize(Dictionary<string, List<string>> result)
        {
            List<string> paths = new List<string>();

            // Clear external rules for each invoke.
            ExternalRules = new List<ExternalRule>();

            // Initialize helper
            Helper.Instance.Initialize();

            // An aggregate catalog that combines multiple catalogs.
            using (AggregateCatalog catalog = new AggregateCatalog())
            {
                // Adds all the parts found in the same directory.
                string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                catalog.Catalogs.Add(new DirectoryCatalog(dirName));

                // Adds user specified directory
                paths = result.ContainsKey("ValidDllPaths") ? result["ValidDllPaths"] : result["ValidPaths"];
                foreach (string path in paths)
                {
                    if (String.Equals(Path.GetExtension(path),".dll",StringComparison.OrdinalIgnoreCase))
                    {
                        catalog.Catalogs.Add(new AssemblyCatalog(path));
                    }
                    else
                    {
                        catalog.Catalogs.Add(new DirectoryCatalog(path));
                    }
                }

                // Creates the CompositionContainer with the parts in the catalog.
                container = new CompositionContainer(catalog);

                // Fills the imports of this object.
                try
                {
                    container.ComposeParts(this);
                }
                catch (CompositionException compositionException)
                {
                    Console.WriteLine(compositionException.ToString());
                }
            }

            // Gets external rules.
            if (result.ContainsKey("ValidModPaths") && result["ValidModPaths"].Count > 0)
                ExternalRules = GetExternalRule(result["ValidModPaths"].ToArray());
        }

        public IEnumerable<IRule> GetRule(string[] moduleNames, string[] ruleNames)
        {
            IEnumerable<IRule> results = null;
            IEnumerable<IExternalRule> externalRules = null;

            // Combines C# rules.
            IEnumerable<IRule> rules = ScriptRules.Union<IRule>(TokenRules)
                                                  .Union<IRule>(DSCResourceRules);

            // Gets PowerShell Rules.
            if (moduleNames != null)
            {
                externalRules = GetExternalRule(moduleNames);
                rules = rules.Union<IRule>(externalRules);
            }

            if (ruleNames != null)
            {
                //Check wild card input for -Name parameter and create regex match patterns
                List<Regex> regexList = new List<Regex>();
                foreach (string ruleName in ruleNames)
                {
                    Regex includeRegex = new Regex(String.Format("^{0}$", Regex.Escape(ruleName).Replace(@"\*", ".*")), RegexOptions.IgnoreCase);
                    regexList.Add(includeRegex);
                }

                results = from rule in rules
                    from regex in regexList
                    where regex.IsMatch(rule.GetName())
                    select rule;
            }
            else
            {
                results = rules;
            }

            return results;
        }

        public List<ExternalRule> GetExternalRule(string[] moduleNames)
        {
            List<ExternalRule> rules = new List<ExternalRule>();

            if (moduleNames == null) return rules;

            // Converts module path to module name.
            foreach (string moduleName in moduleNames)
            {
                string shortModuleName = string.Empty;

                // Imports modules by using full path.
                InitialSessionState state = InitialSessionState.CreateDefault2();
                state.ImportPSModule(new string[] { moduleName });

                using (System.Management.Automation.PowerShell posh =
                       System.Management.Automation.PowerShell.Create(state))
                {
                    string script = string.Format(CultureInfo.CurrentCulture, "Get-Module -Name '{0}' -ListAvailable", moduleName);
                    shortModuleName = posh.AddScript(script).Invoke<PSModuleInfo>().First().Name;

                    // Invokes Get-Command and Get-Help for each functions in the module.
                    script = string.Format(CultureInfo.CurrentCulture, "Get-Command -Module '{0}'", shortModuleName);
                    var psobjects = posh.AddScript(script).Invoke();

                    foreach (PSObject psobject in psobjects)
                    {
                        posh.Commands.Clear();

                        FunctionInfo funcInfo = (FunctionInfo)psobject.ImmediateBaseObject;
                        ParameterMetadata param = funcInfo.Parameters.Values
                            .First<ParameterMetadata>(item => item.Name.EndsWith("ast", StringComparison.OrdinalIgnoreCase) ||
                                item.Name.EndsWith("token", StringComparison.CurrentCulture));
                        
                        //Only add functions that are defined as rules.
                        if (param != null)
                        {
                            script = string.Format(CultureInfo.CurrentCulture, "(Get-Help -Name {0}).Description | Out-String", funcInfo.Name);
                            string desc =posh.AddScript(script).Invoke()[0].ImmediateBaseObject.ToString()
                                    .Replace("\r\n", " ").Trim();

                            rules.Add(new ExternalRule(funcInfo.Name, funcInfo.Name, desc, param.ParameterType.Name,
                                funcInfo.ModuleName, funcInfo.Module.Path));
                        }
                    }
                }
            }

            return rules;
        }

        /// <summary>
        /// GetExternalRecord: Get external rules in parallel using RunspacePool and run each rule in its own runspace.
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="token"></param>
        /// <param name="rules"></param>
        /// <param name="command"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> GetExternalRecord(Ast ast, Token[] token, ExternalRule[] rules, InvokeScriptAnalyzerCommand command, string filePath)
        {
            // Defines InitialSessionState.
            InitialSessionState state = InitialSessionState.CreateDefault2();

            // Groups rules by module paths and imports them.
            Dictionary<string, List<ExternalRule>> modules = rules
                .GroupBy<ExternalRule, string>(item => item.GetFullModulePath())
                .ToDictionary(item => item.Key, item => item.ToList());
            state.ImportPSModule(modules.Keys.ToArray<string>());

            // Creates and opens RunspacePool
            RunspacePool rsp = RunspaceFactory.CreateRunspacePool(state);
            rsp.SetMinRunspaces(1);
            rsp.SetMaxRunspaces(5);
            rsp.Open();

            // Groups rules by AstType and Tokens.
            Dictionary<string, List<ExternalRule>> astRuleGroups = rules
                .Where<ExternalRule>(item => item.GetParameter().EndsWith("ast", true, CultureInfo.CurrentCulture))
                .GroupBy<ExternalRule, string>(item => item.GetParameter())
                .ToDictionary(item => item.Key, item => item.ToList());

            Dictionary<string, List<ExternalRule>> tokenRuleGroups = rules
                .Where<ExternalRule>(item => item.GetParameter().EndsWith("token", true, CultureInfo.CurrentCulture))
                .GroupBy<ExternalRule, string>(item => item.GetParameter())
                .ToDictionary(item => item.Key, item => item.ToList());

            using (rsp)
            {
                // Defines the commands to be run.
                List<System.Management.Automation.PowerShell> powerShellCommands
                    = new List<System.Management.Automation.PowerShell>();

                // Defines the command results.
                List<IAsyncResult> powerShellCommandResults = new List<IAsyncResult>();

                #region Builds and invokes commands list

                foreach (KeyValuePair<string, List<ExternalRule>> tokenRuleGroup in tokenRuleGroups)
                {
                    foreach (IExternalRule rule in tokenRuleGroup.Value)
                    {
                        System.Management.Automation.PowerShell posh =
                            System.Management.Automation.PowerShell.Create();
                        posh.RunspacePool = rsp;

                        // Adds command to run external analyzer rule, like
                        // Measure-CurlyBracket -ScriptBlockAst $ScriptBlockAst
                        // Adds module name (source name) to handle ducplicate function names in different modules.
                        string ruleName = string.Format("{0}\\{1}", rule.GetSourceName(), rule.GetName());
                        posh.Commands.AddCommand(ruleName);
                        posh.Commands.AddParameter(rule.GetParameter(), token);

                        // Merges results because external analyzer rules may throw exceptions.
                        posh.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error,
                            PipelineResultTypes.Output);

                        powerShellCommands.Add(posh);
                        powerShellCommandResults.Add(posh.BeginInvoke());
                    }
                }

                foreach (KeyValuePair<string, List<ExternalRule>> astRuleGroup in astRuleGroups)
                {
                    // Find all AstTypes that appeared in rule groups.
                    IEnumerable<Ast> childAsts = ast.FindAll(new Func<Ast, bool>((testAst) =>
                        (testAst.GetType().Name.ToLower(CultureInfo.CurrentCulture) == astRuleGroup.Key.ToLower(CultureInfo.CurrentCulture))), false);

                    foreach (Ast childAst in childAsts)
                    {
                        foreach (IExternalRule rule in astRuleGroup.Value)
                        {
                            System.Management.Automation.PowerShell posh =
                                System.Management.Automation.PowerShell.Create();
                            posh.RunspacePool = rsp;

                            // Adds command to run external analyzer rule, like
                            // Measure-CurlyBracket -ScriptBlockAst $ScriptBlockAst
                            // Adds module name (source name) to handle ducplicate function names in different modules.
                            string ruleName = string.Format("{0}\\{1}", rule.GetSourceName(), rule.GetName());
                            posh.Commands.AddCommand(ruleName);
                            posh.Commands.AddParameter(rule.GetParameter(), childAst);

                            // Merges results because external analyzer rules may throw exceptions.
                            posh.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error,
                                PipelineResultTypes.Output);

                            powerShellCommands.Add(posh);
                            powerShellCommandResults.Add(posh.BeginInvoke());
                        }
                    }
                }

                #endregion

                #region Collects the results from commands.

                for (int i = 0; i < powerShellCommands.Count; i++)
                {
                    // EndInvoke will wait for each command to finish, so we will be getting the commands
                    // in the same order that they have been invoked withy BeginInvoke.
                    PSDataCollection<PSObject> psobjects = powerShellCommands[i].EndInvoke(powerShellCommandResults[i]);

                    foreach (var psobject in psobjects)
                    {
                        DiagnosticSeverity severity;
                        IScriptExtent extent;
                        string message = string.Empty;
                        string ruleName = string.Empty;

                        if (psobject != null && psobject.ImmediateBaseObject != null)
                        {
                            // Because error stream is merged to output stream,
                            // we need to handle the error records.
                            if (psobject.ImmediateBaseObject is ErrorRecord)
                            {
                                ErrorRecord record = (ErrorRecord)psobject.ImmediateBaseObject;
                                command.WriteError(record);
                                continue;
                            }

                            // DiagnosticRecord may not be correctly returned from external rule.
                            try
                            {
                                Enum.TryParse<DiagnosticSeverity>(psobject.Properties["Severity"].Value.ToString().ToUpper(), out severity);
                                message = psobject.Properties["Message"].Value.ToString();
                                extent = (IScriptExtent)psobject.Properties["Extent"].Value;
                                ruleName = psobject.Properties["RuleName"].Value.ToString();
                            }
                            catch (Exception ex)
                            {
                                command.WriteError(new ErrorRecord(ex, ex.HResult.ToString("X"), ErrorCategory.NotSpecified, this));
                                continue;
                            }

                            if (!string.IsNullOrEmpty(message)) yield return new DiagnosticRecord(message, extent, ruleName, severity, null);
                        }
                    }
                }

                #endregion
            }
        }

        public Dictionary<string, List<string>> CheckRuleExtension(string[] path, PSCmdlet cmdlet)
        {
            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();

            List<string> invalidPaths = new List<string>();
            List<string> validDllPaths = new List<string>();
            List<string> validModPaths = new List<string>();

            // Gets valid module names
            foreach (string childPath in path)
            {
                try
                {
                    cmdlet.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.CheckModuleName, childPath));

                    string resolvedPath = string.Empty;

                    // Users may provide a valid module path or name, 
                    // We have to identify the childPath is really a directory or just a module name.
                    // You can also consider following two commands.
                    //   Get-ScriptAnalyzerRule -RuleExtension "ContosoAnalyzerRules"
                    //   Get-ScriptAnalyzerRule -RuleExtension "%USERPROFILE%\WindowsPowerShell\Modules\ContosoAnalyzerRules"
                    if (Path.GetDirectoryName(childPath) == string.Empty)
                    {
                        resolvedPath = childPath;
                    }
                    else
                    {
                        resolvedPath = cmdlet.SessionState.Path
                            .GetResolvedPSPathFromPSPath(childPath).First().ToString();
                    }

                    using (System.Management.Automation.PowerShell posh =
                           System.Management.Automation.PowerShell.Create())
                    {
                        string script = string.Format(CultureInfo.CurrentCulture, "Get-Module -Name '{0}' -ListAvailable", resolvedPath);
                        PSModuleInfo moduleInfo = posh.AddScript(script).Invoke<PSModuleInfo>().First();

                        // Adds original path, otherwise path.Except<string>(validModPaths) will fail.
                        // It's possible that user can provide something like this:
                        // "..\..\..\ScriptAnalyzer.UnitTest\modules\CommunityAnalyzerRules\CommunityAnalyzerRules.psd1"
                        if (moduleInfo.ExportedFunctions.Count > 0) validModPaths.Add(childPath);
                    }
                }
                catch
                {
                    // User may provide an invalid module name, like c:\temp.
                    // It's a invalid name for a Windows PowerShell module,
                    // But we need test it further since we allow user to provide a folder to extend rules.
                    // You can also consider following two commands.
                    //   Get-ScriptAnalyzerRule -RuleExtension "ContosoAnalyzerRules", "C:\Temp\ExtendScriptAnalyzerRules.dll"
                    //   Get-ScriptAnalyzerRule -RuleExtension "ContosoAnalyzerRules", "C:\Temp\"
                    continue;
                }
            }

            // Gets valid dll paths
            foreach (string childPath in path.Except<string>(validModPaths))
            {
                try
                {
                    string resolvedPath = cmdlet.SessionState.Path
                        .GetResolvedPSPathFromPSPath(childPath).First().ToString();

                    cmdlet.WriteDebug(string.Format(CultureInfo.CurrentCulture, Strings.CheckAssemblyFile, resolvedPath));

                    if (String.Equals(Path.GetExtension(resolvedPath),".dll"))
                    {
                        if (!File.Exists(resolvedPath))
                        {
                            invalidPaths.Add(resolvedPath);
                            continue;
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(resolvedPath))
                        {
                            invalidPaths.Add(resolvedPath);
                            continue;
                        }
                    }

                    validDllPaths.Add(resolvedPath);
                }
                catch
                {
                    invalidPaths.Add(childPath);
                }
            }

            // Resloves relative paths.
            try
            {
                for (int i = 0; i < validModPaths.Count; i++)
                {
                    validModPaths[i] = cmdlet.SessionState.Path
                        .GetResolvedPSPathFromPSPath(validModPaths[i]).First().ToString();
                }
                for (int i = 0; i < validDllPaths.Count; i++)
                {
                    validDllPaths[i] = cmdlet.SessionState.Path
                        .GetResolvedPSPathFromPSPath(validDllPaths[i]).First().ToString();
                }
            }
            catch
            {
                // If GetResolvedPSPathFromPSPath failed. We can safely ignore the exception.
                // Because GetResolvedPSPathFromPSPath always fails when trying to resolve a module name.
            }

            // Returns valid rule extensions
            results.Add("InvalidPaths", invalidPaths);
            results.Add("ValidModPaths", validModPaths);
            results.Add("ValidDllPaths", validDllPaths);

            return results;
        }

        #endregion
    }
}
