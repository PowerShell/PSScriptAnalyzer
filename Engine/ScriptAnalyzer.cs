
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
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
#endif // !CORECLR
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Globalization;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public sealed class ScriptAnalyzer
    {
        #region Private members

        private IOutputWriter outputWriter;
        private Dictionary<string, object> settings;
#if !CORECLR
        private CompositionContainer container;
#endif // !CORECLR
        Dictionary<string, List<string>> validationResults = new Dictionary<string, List<string>>();
        string[] includeRule;
        string[] excludeRule;
        string[] severity;
        List<Regex> includeRegexList;
        List<Regex> excludeRegexList;
        bool suppressedOnly;
#if !PSV3
        ModuleDependencyHandler moduleHandler;
#endif
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

#if CORECLR
        public IEnumerable<IScriptRule> ScriptRules { get; private set; }
        public IEnumerable<ITokenRule> TokenRules { get; private set; }
        public IEnumerable<ILogger> Loggers { get; private set; }
        public IEnumerable<IDSCResourceRule> DSCResourceRules { get; private set; }
#else
        [ImportMany]
        public IEnumerable<IScriptRule> ScriptRules { get; private set; }
        [ImportMany]
        public IEnumerable<ITokenRule> TokenRules { get; private set; }
        [ImportMany]
        public IEnumerable<ILogger> Loggers { get; private set; }
        [ImportMany]
        public IEnumerable<IDSCResourceRule> DSCResourceRules { get; private set; }
        // Initializes via ImportMany
#endif // !CORECLR

        internal List<ExternalRule> ExternalRules { get; set; }

#if !PSV3
        public ModuleDependencyHandler ModuleHandler {
            get
            {
                return moduleHandler;
            }

            internal set
            {
                moduleHandler = value;
            }
        }
#endif

#endregion

#region Methods

        /// <summary>
        /// Initialize : Initializes default rules, loggers and helper.
        /// </summary>
        internal void Initialize<TCmdlet>(
            TCmdlet cmdlet,
            string[] customizedRulePath = null,
            string[] includeRuleNames = null,
            string[] excludeRuleNames = null,
            string[] severity = null,
            bool includeDefaultRules = false,
            bool suppressedOnly = false)
            where TCmdlet : PSCmdlet, IOutputWriter
        {
            if (cmdlet == null)
            {
                throw new ArgumentNullException("cmdlet");
            }

            this.Initialize(
                cmdlet,
                cmdlet.SessionState.Path,
                cmdlet.SessionState.InvokeCommand,
                customizedRulePath,
                includeRuleNames,
                excludeRuleNames,
                severity,
                includeDefaultRules,
                suppressedOnly);
        }

        /// <summary>
        /// Initialize : Initializes default rules, loggers and helper.
        /// </summary>
        public void Initialize(
            Runspace runspace,
            IOutputWriter outputWriter,
            string[] customizedRulePath = null,
            string[] includeRuleNames = null,
            string[] excludeRuleNames = null,
            string[] severity = null,
            bool includeDefaultRules = false,
            bool suppressedOnly = false,
            string profile = null)
        {
            if (runspace == null)
            {
                throw new ArgumentNullException("runspace");
            }

            //initialize helper
            Helper.Instance = new Helper(
                runspace.SessionStateProxy.InvokeCommand,
                outputWriter);
            Helper.Instance.Initialize();


            this.Initialize(
                outputWriter,
                runspace.SessionStateProxy.Path,
                runspace.SessionStateProxy.InvokeCommand,
                customizedRulePath,
                includeRuleNames,
                excludeRuleNames,
                severity,
                includeDefaultRules,
                suppressedOnly,
                profile);
        }

        /// <summary>
        /// clean up this instance, resetting all properties
        /// </summary>
        public void CleanUp()
        {
            includeRule = null;
            excludeRule = null;
            severity = null;
            includeRegexList = null;
            excludeRegexList = null;
            suppressedOnly = false;
        }

        internal bool ParseProfile(object profileObject, PathIntrinsics path, IOutputWriter writer)
        {
            // profile was not given
            if (profileObject == null)
            {
                writer.WriteVerbose("No settings hashtable or file to consume");
                return true;
            }

            if (!(profileObject is string || profileObject is Hashtable))
            {
                writer.WriteWarning("Settings parameter should be a hashtable or a filepath");
                return false;
            }

            List<string> includeRuleList = new List<string>();
            List<string> excludeRuleList = new List<string>();
            List<string> severityList = new List<string>();

            bool hasError = false;

            Hashtable hashTableProfile = profileObject as Hashtable;

            // checks whether we get a hashtable
            if (hashTableProfile != null)
            {
                hasError = ParseProfileHashtable(hashTableProfile, path, writer, severityList, includeRuleList, excludeRuleList);
            }
            else
            {
                // checks whether we get a string instead
                string profile = profileObject as string;

                if (!String.IsNullOrWhiteSpace(profile))
                {
                    hasError = ParseProfileString(profile, path, writer, severityList, includeRuleList, excludeRuleList);
                }
            }

            if (hasError)
            {
                return false;
            }

            this.severity = (severityList.Count() == 0) ? null : severityList.ToArray();
            this.includeRule = (includeRuleList.Count() == 0) ? null : includeRuleList.ToArray();
            this.excludeRule = (excludeRuleList.Count() == 0) ? null : excludeRuleList.ToArray();
            if (settings != null
                && settings.ContainsKey("Rules"))
            {
                var ruleArgs = settings["Rules"] as Dictionary<string, object>;
                Helper.Instance.SetRuleArguments(ruleArgs);
            }
            return true;
        }

        private bool AddProfileItem(
            string key,
            List<string> values,
            List<string> severityList,
            List<string> includeRuleList,
            List<string> excludeRuleList)
        {
            Debug.Assert(key != null);
            Debug.Assert(values != null);
            Debug.Assert(severityList != null);
            Debug.Assert(includeRuleList != null);
            Debug.Assert(excludeRuleList != null);

            switch (key.ToLower())
            {
                case "severity":
                    severityList.AddRange(values);
                    break;
                case "includerules":
                    includeRuleList.AddRange(values);
                    break;
                case "excluderules":
                    excludeRuleList.AddRange(values);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private Dictionary<string, object> GetDictionaryFromHashTableAst(
            HashtableAst hashTableAst,
            IOutputWriter writer,
            string profile,
            out bool hasError)
        {
            hasError = false;
            var output = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in hashTableAst.KeyValuePairs)
            {
                var keyAst = kvp.Item1 as StringConstantExpressionAst;
                if (keyAst == null)
                {
                    // first item (the key) should be a string
                    writer.WriteError(
                        new ErrorRecord(
                            new InvalidDataException(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.WrongKeyFormat,
                                    kvp.Item1.Extent.StartLineNumber,
                                    kvp.Item1.Extent.StartColumnNumber,
                                    profile)),
                            Strings.ConfigurationKeyNotAString,
                            ErrorCategory.InvalidData,
                            profile));
                    hasError = true;
                    continue;
                }
                var key = keyAst.Value;
                // parse the item2 as array
                PipelineAst pipeAst = kvp.Item2 as PipelineAst;
                List<string> rhsList = new List<string>();
                if (pipeAst != null)
                {
                    ExpressionAst pureExp = pipeAst.GetPureExpression();
                    if (pureExp is StringConstantExpressionAst)
                    {
                        rhsList.Add((pureExp as StringConstantExpressionAst).Value);
                    }
                    else if (pureExp is HashtableAst)
                    {
                        output[key] = GetDictionaryFromHashTableAst(
                            pureExp as HashtableAst,
                            writer,
                            profile,
                            out hasError);
                        continue;
                    }
                    else
                    {
                        ArrayLiteralAst arrayLitAst = pureExp as ArrayLiteralAst;
                        if (arrayLitAst == null && pureExp is ArrayExpressionAst)
                        {
                            ArrayExpressionAst arrayExp = pureExp as ArrayExpressionAst;
                            // Statements property is never null
                            if (arrayExp.SubExpression != null)
                            {
                                StatementAst stateAst = arrayExp.SubExpression.Statements.FirstOrDefault();
                                if (stateAst != null && stateAst is PipelineAst)
                                {
                                    CommandBaseAst cmdBaseAst = (stateAst as PipelineAst).PipelineElements.FirstOrDefault();
                                    if (cmdBaseAst != null && cmdBaseAst is CommandExpressionAst)
                                    {
                                        CommandExpressionAst cmdExpAst = cmdBaseAst as CommandExpressionAst;
                                        if (cmdExpAst.Expression is StringConstantExpressionAst)
                                        {
                                            rhsList.Add((cmdExpAst.Expression as StringConstantExpressionAst).Value);
                                        }
                                        else
                                        {
                                            arrayLitAst = cmdExpAst.Expression as ArrayLiteralAst;
                                        }
                                    }
                                }
                            }
                        }

                        if (arrayLitAst != null)
                        {
                            foreach (var element in arrayLitAst.Elements)
                            {
                                // all the values in the array needs to be string
                                if (!(element is StringConstantExpressionAst))
                                {
                                    outputWriter.WriteError(
                                        new ErrorRecord(
                                            new InvalidDataException(
                                                string.Format(
                                                    CultureInfo.CurrentCulture,
                                                    Strings.WrongValueFormat,
                                                    element.Extent.StartLineNumber,
                                                    element.Extent.StartColumnNumber,
                                                    "")),
                                        Strings.ConfigurationValueNotAString,
                                        ErrorCategory.InvalidData,
                                        null));
                                    hasError = true;
                                    continue;
                                }

                                rhsList.Add((element as StringConstantExpressionAst).Value);
                            }
                        }
                    }
                }

                if (rhsList.Count == 0)
                {
                    writer.WriteError(
                        new ErrorRecord(
                            new InvalidDataException(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.WrongValueFormat,
                                    kvp.Item2.Extent.StartLineNumber,
                                    kvp.Item2.Extent.StartColumnNumber,
                                    profile)),
                            Strings.ConfigurationValueWrongFormat,
                            ErrorCategory.InvalidData,
                            profile));
                    hasError = true;
                    continue;
                }
                output[key] = rhsList;
            }
            return output;
        }

        /// <summary>
        /// Recursively convert hashtable to dictionary
        /// </summary>
        /// <param name="hashtable"></param>
        /// <returns>Dictionary that maps string to object</returns>
        private Dictionary<string, object> GetDictionaryFromHashtable(
            Hashtable hashtable,
            IOutputWriter writer,
            out bool hasError)
        {
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            hasError = false;
            foreach (var obj in hashtable.Keys)
            {
                string key = obj as string;
                if (key == null)
                {
                    writer.WriteError(
                        new ErrorRecord(
                            new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.KeyNotString, key)),
                            Strings.ConfigurationKeyNotAString,
                            ErrorCategory.InvalidData,
                            hashtable));
                    hasError = true;
                    return null;
                }
                var valueHashtableObj = hashtable[obj];
                if (valueHashtableObj == null)
                {
                    writer.WriteError(
                        new ErrorRecord(
                            new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, valueHashtableObj, key)),
                            Strings.WrongConfigurationKey,
                            ErrorCategory.InvalidData,
                            hashtable));
                    hasError = true;
                    return null;
                }
                var valueHashtable = valueHashtableObj as Hashtable;
                if (valueHashtable == null)
                {
                    dictionary.Add(key, valueHashtableObj);
                }
                else
                {
                    var dict = GetDictionaryFromHashtable(valueHashtable, writer, out hasError);
                    if (dict != null)
                    {
                        dictionary.Add(key, dict);
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            return dictionary;
        }

        private bool ParseProfileHashtable(Hashtable profile, PathIntrinsics path, IOutputWriter writer,
            List<string> severityList, List<string> includeRuleList, List<string> excludeRuleList)
        {
            bool hasError = false;

            HashSet<string> validKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            validKeys.Add("severity");
            validKeys.Add("includerules");
            validKeys.Add("excluderules");
            validKeys.Add("rules");

            settings = GetDictionaryFromHashtable(profile, writer, out hasError);
            if (hasError)
            {
                return hasError;
            }

            var settingsKeys = new string[settings.Keys.Count];
            settings.Keys.CopyTo(settingsKeys, 0);
            foreach (var settingKey in settingsKeys)
            {
                var key = settingKey.ToLower();
                object value = settings[key];
                switch (key)
                {
                    case "severity":
                    case "includerules":
                    case "excluderules":
                        // value must be either string or collections of string or array
                        if (value == null
                            || !(value is string
                                || value is IEnumerable<string>
                                || value.GetType().IsArray))
                        {
                            writer.WriteError(
                                new ErrorRecord(
                                    new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, value, key)),
                                    Strings.WrongConfigurationKey,
                                    ErrorCategory.InvalidData,
                                    profile));
                            hasError = true;
                            break;
                        }
                        List<string> values = new List<string>();
                        if (value is string)
                        {
                            values.Add(value as string);
                        }
                        else if (value is IEnumerable<string>)
                        {
                            values.Union(value as IEnumerable<string>);
                        }
                        else if (value.GetType().IsArray)
                        {
                            // for array case, sometimes we won't be able to cast it directly to IEnumerable<string>
                            foreach (var val in value as IEnumerable)
                            {
                                if (val is string)
                                {
                                    values.Add(val as string);
                                }
                                else
                                {
                                    writer.WriteError(
                                        new ErrorRecord(
                                            new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongValueHashTable, val, key)),
                                            Strings.WrongConfigurationKey,
                                            ErrorCategory.InvalidData,
                                            profile));
                                    hasError = true;
                                    break;
                                }
                            }
                        }
                        AddProfileItem(key, values, severityList, includeRuleList, excludeRuleList);
                        settings[key] = values;
                        break;

                    case "rules":
                        break;

                    default:
                        writer.WriteError(
                            new ErrorRecord(
                                new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Strings.WrongKeyHashTable, key)),
                                Strings.WrongConfigurationKey,
                                ErrorCategory.InvalidData,
                                profile));
                        hasError = true;
                        break;
                }
            }
            return hasError;
        }

        private bool ParseProfileString(string profile, PathIntrinsics path, IOutputWriter writer,
            List<string> severityList, List<string> includeRuleList, List<string> excludeRuleList)
        {
            bool hasError = false;

            try
            {
                profile = path.GetResolvedPSPathFromPSPath(profile).First().Path;
            }
            catch
            {
                writer.WriteError(new ErrorRecord(new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Strings.FileNotFound, profile)),
                    Strings.ConfigurationFileNotFound, ErrorCategory.ResourceUnavailable, profile));
                hasError = true;
            }

            if (File.Exists(profile))
            {
                Token[] parserTokens = null;
                ParseError[] parserErrors = null;
                Ast profileAst = Parser.ParseFile(profile, out parserTokens, out parserErrors);
                IEnumerable<Ast> hashTableAsts = profileAst.FindAll(item => item is HashtableAst, false);

                // no hashtable, raise warning
                if (hashTableAsts.Count() == 0)
                {
                    writer.WriteError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidProfile, profile)),
                        Strings.ConfigurationFileHasNoHashTable, ErrorCategory.ResourceUnavailable, profile));
                    hasError = true;
                }
                else
                {
                    HashtableAst hashTableAst = hashTableAsts.First() as HashtableAst;
#if PSV3
                    settings = GetDictionaryFromHashTableAst(
                        hashTableAst,
                        writer,
                        profile,
                        out hasError);
                    foreach (var key in settings.Keys)
                    {
                        var rhsList = settings[key] as List<string>;
                        if (rhsList == null)
                        {
                            continue;
                        }
                        if (!AddProfileItem(key, rhsList, severityList, includeRuleList, excludeRuleList))
                        {
                            writer.WriteError(
                                new ErrorRecord(
                                    new InvalidDataException(
                                        string.Format(
                                            CultureInfo.CurrentCulture,
                                            Strings.WrongKey,
                                            key,
                                            profile)),
                                    Strings.WrongConfigurationKey,
                                    ErrorCategory.InvalidData,
                                    profile));
                            hasError = true;
                        }
                    }
#else

                    try
                    {
                        var hashtable = hashTableAst.SafeGetValue() as Hashtable;
                        hasError = ParseProfileHashtable(hashtable, path, writer, severityList, includeRuleList, excludeRuleList);
                    }
                    catch
                    {
                        writer.WriteError(
                            new ErrorRecord(
                                new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidProfile, profile)),
                                Strings.ConfigurationFileHasInvalidHashtable,
                                ErrorCategory.ResourceUnavailable,
                                profile));
                        hasError = true;
                    }
#endif // PSV3
                }
            }

            return hasError;
        }

        private void Initialize(
            IOutputWriter outputWriter,
            PathIntrinsics path,
            CommandInvocationIntrinsics invokeCommand,
            string[] customizedRulePath,
            string[] includeRuleNames,
            string[] excludeRuleNames,
            string[] severity,
            bool includeDefaultRules = false,
            bool suppressedOnly = false,
            string profile = null)
        {
            if (outputWriter == null)
            {
                throw new ArgumentNullException("outputWriter");
            }
#if !PSV3
            this.moduleHandler = null;
#endif

            this.outputWriter = outputWriter;

#region Verifies rule extensions

            List<string> paths = this.GetValidCustomRulePaths(customizedRulePath, path);

            #endregion

#if CORECLR
            // For Full CLR logger is loaded via Composition
            // But for Core CLR we need to load it explicitly
            this.Loggers = GetInterfaceImplementationsFromAssembly<ILogger>();
#endif
            #region Initializes Rules

            var includeRuleList = new List<string>();
            var excludeRuleList = new List<string>();
            var severityList = new List<string>();

            if (profile != null)
            {
                ParseProfileString(profile, path, outputWriter, severityList, includeRuleList, excludeRuleList);
            }

            if (includeRuleNames != null)
            {
                foreach (string includeRuleName in includeRuleNames.Where(rule => !includeRuleList.Contains(rule, StringComparer.OrdinalIgnoreCase)))
                {
                    includeRuleList.Add(includeRuleName);
                }
            }

            if (excludeRuleNames != null)
            {
                foreach (string excludeRuleName in excludeRuleNames.Where(rule => !excludeRuleList.Contains(rule, StringComparer.OrdinalIgnoreCase)))
                {
                    excludeRuleList.Add(excludeRuleName);
                }
            }

            if (severity != null)
            {
                foreach (string sev in severity.Where(s => !severityList.Contains(s, StringComparer.OrdinalIgnoreCase)))
                {
                    severityList.Add(sev);
                }
            }

            this.suppressedOnly = suppressedOnly;
            this.includeRegexList = new List<Regex>();
            this.excludeRegexList = new List<Regex>();

            if (this.severity == null)
            {
                this.severity = severityList.Count == 0 ? null : severityList.ToArray();
            }
            else
            {
                this.severity = this.severity.Union(severityList).ToArray();
            }

            if (this.includeRule == null)
            {
                this.includeRule = includeRuleList.Count == 0 ? null : includeRuleList.ToArray();
            }
            else
            {
                this.includeRule = this.includeRule.Union(includeRuleList).ToArray();
            }

            if (this.excludeRule == null)
            {
                this.excludeRule = excludeRuleList.Count == 0 ? null : excludeRuleList.ToArray();
            }
            else
            {
                this.excludeRule = this.excludeRule.Union(excludeRuleList).ToArray();
            }

            //Check wild card input for the Include/ExcludeRules and create regex match patterns
            if (this.includeRule != null)
            {
                foreach (string rule in includeRule)
                {
                    Regex includeRegex = new Regex(String.Format("^{0}$", Regex.Escape(rule).Replace(@"\*", ".*")), RegexOptions.IgnoreCase);
                    this.includeRegexList.Add(includeRegex);
                }
            }

            if (this.excludeRule != null)
            {
                foreach (string rule in excludeRule)
                {
                    Regex excludeRegex = new Regex(String.Format("^{0}$", Regex.Escape(rule).Replace(@"\*", ".*")), RegexOptions.IgnoreCase);
                    this.excludeRegexList.Add(excludeRegex);
                }
            }

            try
            {
                this.LoadRules(this.validationResults, invokeCommand, includeDefaultRules);
            }
            catch (Exception ex)
            {
                this.outputWriter.ThrowTerminatingError(
                    new ErrorRecord(
                        ex,
                        ex.HResult.ToString("X", CultureInfo.CurrentCulture),
                        ErrorCategory.NotSpecified, this));
            }

#endregion

#region Verify rules

            // Safely get one non-duplicated list of rules
            IEnumerable<IRule> rules =
                Enumerable.Union<IRule>(
                    Enumerable.Union<IRule>(
                        this.ScriptRules ?? Enumerable.Empty<IRule>(),
                        this.TokenRules ?? Enumerable.Empty<IRule>()),
                    this.ExternalRules ?? Enumerable.Empty<IExternalRule>());

            // Ensure that rules were actually loaded
            if (rules == null || rules.Any() == false)
            {
                this.outputWriter.ThrowTerminatingError(
                    new ErrorRecord(
                        new Exception(),
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.RulesNotFound),
                        ErrorCategory.ResourceExists,
                        this));
            }

#endregion
        }

        private List<string> GetValidCustomRulePaths(string[] customizedRulePath, PathIntrinsics path)
        {
            List<string> paths = new List<string>();

            if (customizedRulePath != null)
            {
                paths.AddRange(
                    customizedRulePath.ToList());
            }

            if (paths.Count > 0)
            {
                this.validationResults = this.CheckRuleExtension(paths.ToArray(), path);
                foreach (string extension in this.validationResults["InvalidPaths"])
                {
                    this.outputWriter.WriteWarning(string.Format(CultureInfo.CurrentCulture, Strings.MissingRuleExtension, extension));
                }
            }
            else
            {
                this.validationResults = new Dictionary<string, List<string>>();
                this.validationResults.Add("InvalidPaths", new List<string>());
                this.validationResults.Add("ValidModPaths", new List<string>());
                this.validationResults.Add("ValidDllPaths", new List<string>());
            }

            return paths;
        }


        private IEnumerable<T> GetInterfaceImplementationsFromAssembly<T>() where T : class
        {
            string dirName = Path.GetDirectoryName(typeof(ScriptAnalyzer).GetTypeInfo().Assembly.Location);
            var dllPaths = Directory.EnumerateFiles(dirName, "*.dll", SearchOption.TopDirectoryOnly);
            var objects = new List<T>();
            foreach (var dllPath in dllPaths)
            {
                outputWriter.WriteVerbose(string.Format("Found Assembly: {0}", dllPath));
                var objectsFromOneFile = GetInterfaceImplementationsFromAssembly<T>(dllPath);
                objects.AddRange(objectsFromOneFile);
            }
            return objects;
        }

        private IEnumerable<T> GetInterfaceImplementationsFromAssembly<T>(string ruleDllPath) where T : class
        {
            var fileName = Path.GetFileNameWithoutExtension(ruleDllPath);
            var assemblyName = new AssemblyName(fileName);
            outputWriter.WriteVerbose(string.Format("Loading Assembly:{0}", assemblyName.FullName));

            var dll = Assembly.Load(assemblyName);
            var rules = new List<T>();
            if (dll == null)
            {
                outputWriter.WriteVerbose(string.Format("Cannot load {0}", ruleDllPath));
                return rules;
            }
            foreach (var type in dll.ExportedTypes)
            {
                var typeInfo = type.GetTypeInfo();
                if (!typeInfo.IsInterface
                    && !typeInfo.IsAbstract
                    && typeInfo.ImplementedInterfaces.Contains(typeof(T)))
                {
                    outputWriter.WriteVerbose(
                        string.Format(
                            "Creating Instance of {0}", type.Name));

                    var ruleObj = Activator.CreateInstance(type);
                    T rule = ruleObj as T;
                    if (rule == null)
                    {
                        outputWriter.WriteVerbose(
                            string.Format(
                                "Cannot cast instance of type {0} to {1}", type.Name, typeof(T).GetTypeInfo().Name));
                        continue;
                    }
                    rules.Add(rule);
                }
            }
            return rules;
        }

        private void LoadRules(Dictionary<string, List<string>> result, CommandInvocationIntrinsics invokeCommand, bool loadBuiltInRules)
        {
            List<string> paths = new List<string>();

            // Clear external rules for each invoke.
            this.ScriptRules = null;
            this.TokenRules = null;
            this.ExternalRules = null;

#if CORECLR
            this.ScriptRules = GetInterfaceImplementationsFromAssembly<IScriptRule>();
            this.TokenRules = GetInterfaceImplementationsFromAssembly<ITokenRule>();
            this.DSCResourceRules = GetInterfaceImplementationsFromAssembly<IDSCResourceRule>();
#else
            // An aggregate catalog that combines multiple catalogs.
            using (AggregateCatalog catalog = new AggregateCatalog())
            {
                // Adds all the parts found in the same directory.
                string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                catalog.Catalogs.Add(
                    new SafeDirectoryCatalog(
                        dirName,
                        this.outputWriter));

                // Adds user specified directory
                paths = result.ContainsKey("ValidDllPaths") ? result["ValidDllPaths"] : result["ValidPaths"];
                foreach (string path in paths)
                {
                    if (String.Equals(Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        catalog.Catalogs.Add(new AssemblyCatalog(path));
                    }
                    else
                    {
                        catalog.Catalogs.Add(
                            new SafeDirectoryCatalog(
                                path,
                                this.outputWriter));
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
                    this.outputWriter.WriteWarning(compositionException.ToString());
                }
            }
#endif // CORECLR
            if (!loadBuiltInRules)
            {
                this.ScriptRules = null;
            }

            // Gets external rules.
            if (result.ContainsKey("ValidModPaths") && result["ValidModPaths"].Count > 0)
            {
                ExternalRules = GetExternalRule(result["ValidModPaths"].ToArray());
            }
        }

        internal string[] GetValidModulePaths()
        {
            List<string> validModulePaths = null;

            if (!this.validationResults.TryGetValue("ValidModPaths", out validModulePaths))
            {
                validModulePaths = new List<string>();
            }

            return validModulePaths.ToArray();
        }

        public IEnumerable<IRule> GetRule(string[] moduleNames, string[] ruleNames)
        {
            IEnumerable<IRule> results = null;
            IEnumerable<IExternalRule> externalRules = null;

            // Combines C# rules.
            IEnumerable<IRule> rules = Enumerable.Empty<IRule>();

            if (null != ScriptRules)
            {
                rules = ScriptRules.Union<IRule>(TokenRules).Union<IRule>(DSCResourceRules);
            }

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

        private List<ExternalRule> GetExternalRule(string[] moduleNames)
        {
            List<ExternalRule> rules = new List<ExternalRule>();

            if (moduleNames == null) return rules;

            // Converts module path to module name.
            foreach (string moduleName in moduleNames)
            {
                string shortModuleName = string.Empty;

                // Imports modules by using full path.
                InitialSessionState state = InitialSessionState.CreateDefault2();
                using (System.Management.Automation.PowerShell posh =
                       System.Management.Automation.PowerShell.Create(state))
                {
                    posh.AddCommand("Import-Module").AddArgument(moduleName).AddParameter("PassThru");
                    Collection<PSModuleInfo> loadedModules = posh.Invoke<PSModuleInfo>();
                    if (loadedModules != null && loadedModules.Count > 0)
                    {
                        shortModuleName = loadedModules.First().Name;
                    }

                    // Invokes Get-Command and Get-Help for each functions in the module.
                    posh.Commands.Clear();
                    posh.AddCommand("Get-Command").AddParameter("Module", shortModuleName);
                    var psobjects = posh.Invoke();

                    foreach (PSObject psobject in psobjects)
                    {
                        posh.Commands.Clear();

                        FunctionInfo funcInfo = (FunctionInfo)psobject.ImmediateBaseObject;
                        ParameterMetadata param = null;

                        // Ignore any exceptions associated with finding functions that are ScriptAnalyzer rules
                        try
                        {
                            param = funcInfo.Parameters.Values.First<ParameterMetadata>(item => item.Name.EndsWith("ast", StringComparison.OrdinalIgnoreCase) ||
                                                                                                          item.Name.EndsWith("token", StringComparison.OrdinalIgnoreCase));
                        }
                        catch
                        {
                        }

                        //Only add functions that are defined as rules.
                        if (param != null)
                        {
                            // On a new image, when Get-Help is run the first time, PowerShell offers to download updated help content
                            // using Update-Help. This results in an interactive prompt - which we cannot handle
                            // Workaround to prevent Update-Help from running is to set the following reg key
                            // HKLM:\Software\Microsoft\PowerShell\DisablePromptToUpdateHelp
                            // OR execute Update-Help in an elevated admin mode before running ScriptAnalyzer
                            Collection<PSObject> helpContent = null;
                            try
                            {
                                posh.AddCommand("Get-Help").AddParameter("Name", funcInfo.Name);
                                helpContent = posh.Invoke();
                            }
                            catch (Exception getHelpException)
                            {
                                this.outputWriter.WriteWarning(getHelpException.Message.ToString());
                            }

                            // Retrieve "Description" field in the help content
                            string desc = String.Empty;

                            if ((null != helpContent) && ( 1 == helpContent.Count))
                            {
                                dynamic description = helpContent[0].Properties["Description"];

                                if (null != description && null != description.Value && description.Value.GetType().IsArray)
                                {
                                    desc = description.Value[0].Text;
                                }
                            }

                            rules.Add(new ExternalRule(funcInfo.Name, funcInfo.Name, desc, param.Name, param.ParameterType.FullName,
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
        internal IEnumerable<DiagnosticRecord> GetExternalRecord(Ast ast, Token[] token, ExternalRule[] rules, string filePath)
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
                .Where<ExternalRule>(item => item.GetParameter().EndsWith("ast", StringComparison.OrdinalIgnoreCase))
                .GroupBy<ExternalRule, string>(item => item.GetParameterType())
                .ToDictionary(item => item.Key, item => item.ToList());

            Dictionary<string, List<ExternalRule>> tokenRuleGroups = rules
                .Where<ExternalRule>(item => item.GetParameter().EndsWith("token", StringComparison.OrdinalIgnoreCase))
                .GroupBy<ExternalRule, string>(item => item.GetParameterType())
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
                        (astRuleGroup.Key.IndexOf(testAst.GetType().FullName, StringComparison.OrdinalIgnoreCase) != -1)), true);

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
                List<DiagnosticRecord> diagnostics = new List<DiagnosticRecord>();
                try
                {
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
                                    this.outputWriter.WriteError(record);
                                    continue;
                                }

                                // DiagnosticRecord may not be correctly returned from external rule.
                                try
                                {
                                    severity = (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), psobject.Properties["Severity"].Value.ToString());
                                    message = psobject.Properties["Message"].Value.ToString();
                                    extent = (IScriptExtent)psobject.Properties["Extent"].Value;
                                    ruleName = psobject.Properties["RuleName"].Value.ToString();
                                }
                                catch (Exception ex)
                                {
                                    this.outputWriter.WriteError(new ErrorRecord(ex, ex.HResult.ToString("X"), ErrorCategory.NotSpecified, this));
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(message))
                                {
                                    diagnostics.Add(new DiagnosticRecord(message, extent, ruleName, severity, filePath));
                                }
                            }
                        }
                    }
                }
                //Catch exception where customized defined rules have exceptins when doing invoke
                catch (Exception ex)
                {
                    this.outputWriter.WriteError(new ErrorRecord(ex, ex.HResult.ToString("X"), ErrorCategory.NotSpecified, this));
                }

                return diagnostics;
#endregion
            }
        }

        public Dictionary<string, List<string>> CheckRuleExtension(string[] path, PathIntrinsics basePath)
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
                    this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.CheckModuleName, childPath));

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
                        resolvedPath = basePath
                            .GetResolvedPSPathFromPSPath(childPath).First().ToString();
                    }

                    // Import the module
                    InitialSessionState state = InitialSessionState.CreateDefault2();
                    using (System.Management.Automation.PowerShell posh =
                           System.Management.Automation.PowerShell.Create(state))
                    {
                        posh.AddCommand("Import-Module").AddArgument(resolvedPath).AddParameter("PassThru");
                        Collection<PSModuleInfo> loadedModules = posh.Invoke<PSModuleInfo>();
                        if (loadedModules != null
                                && loadedModules.Count > 0
                                && loadedModules.First().ExportedFunctions.Count > 0)
                        {
                                validModPaths.Add(resolvedPath);
                        }
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
                    string resolvedPath = basePath
                        .GetResolvedPSPathFromPSPath(childPath).First().ToString();

                    this.outputWriter.WriteDebug(string.Format(CultureInfo.CurrentCulture, Strings.CheckAssemblyFile, resolvedPath));

                    if (String.Equals(Path.GetExtension(resolvedPath), ".dll", StringComparison.OrdinalIgnoreCase))
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

            // Resolves relative paths.
            try
            {
                for (int i = 0; i < validModPaths.Count; i++)
                {
                    validModPaths[i] = basePath
                        .GetResolvedPSPathFromPSPath(validModPaths[i]).First().ToString();
                }
                for (int i = 0; i < validDllPaths.Count; i++)
                {
                    validDllPaths[i] = basePath
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


        /// <summary>
        /// Analyzes a script file or a directory containing script files.
        /// </summary>
        /// <param name="path">The path of the file or directory to analyze.</param>
        /// <param name="searchRecursively">
        /// If true, recursively searches the given file path and analyzes any
        /// script files that are found.
        /// </param>
        /// <returns>An enumeration of DiagnosticRecords that were found by rules.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzePath(string path, bool searchRecursively = false)
        {
            List<string> scriptFilePaths = new List<string>();

            if (path == null)
            {
                this.outputWriter.ThrowTerminatingError(
                    new ErrorRecord(
                        new FileNotFoundException(),
                        string.Format(CultureInfo.CurrentCulture, Strings.FileNotFound, path),
                        ErrorCategory.InvalidArgument,
                        this));
            }

            // Create in advance the list of script file paths to analyze.  This
            // is an optimization over doing the whole operation at once
            // and calling .Concat on IEnumerables to join results.
            this.BuildScriptPathList(path, searchRecursively, scriptFilePaths);
            foreach (string scriptFilePath in scriptFilePaths)
            {
                // Yield each record in the result so that the
                // caller can pull them one at a time
                foreach (var diagnosticRecord in this.AnalyzeFile(scriptFilePath))
                {
                    yield return diagnosticRecord;
                }
            }
        }

        /// <summary>
        /// Analyzes a script definition in the form of a string input
        /// </summary>
        /// <param name="scriptDefinition">The script to be analyzed</param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScriptDefinition(string scriptDefinition)
        {
            ScriptBlockAst scriptAst = null;
            Token[] scriptTokens = null;
            ParseError[] errors = null;

            this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseScriptDefinitionMessage));

            try
            {
                scriptAst = Parser.ParseInput(scriptDefinition, out scriptTokens, out errors);
            }
            catch (Exception e)
            {
                this.outputWriter.WriteWarning(e.ToString());
                return null;
            }

            if (errors != null && errors.Length > 0)
            {
                foreach (ParseError error in errors)
                {
                    string parseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParseErrorFormatForScriptDefinition, error.Message.TrimEnd('.'), error.Extent.StartLineNumber, error.Extent.StartColumnNumber);
                    this.outputWriter.WriteError(new ErrorRecord(new ParseException(parseErrorMessage), parseErrorMessage, ErrorCategory.ParserError, error.ErrorId));
                }
            }

            if (errors != null && errors.Length > 10)
            {
                string manyParseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParserErrorMessageForScriptDefinition);
                this.outputWriter.WriteError(new ErrorRecord(new ParseException(manyParseErrorMessage), manyParseErrorMessage, ErrorCategory.ParserError, scriptDefinition));

                return new List<DiagnosticRecord>();
            }

            return this.AnalyzeSyntaxTree(scriptAst, scriptTokens, String.Empty);
        }

        private void BuildScriptPathList(
            string path,
            bool searchRecursively,
            IList<string> scriptFilePaths)
        {
            const string ps1Suffix = ".ps1";
            const string psm1Suffix = ".psm1";
            const string psd1Suffix = ".psd1";

            if (Directory.Exists(path))
            {
                if (searchRecursively)
                {
                    foreach (string filePath in Directory.GetFiles(path))
                    {
                        this.BuildScriptPathList(filePath, searchRecursively, scriptFilePaths);
                    }
                    foreach (string filePath in Directory.GetDirectories(path))
                    {
                        this.BuildScriptPathList(filePath, searchRecursively, scriptFilePaths);
                    }
                }
                else
                {
                    foreach (string filePath in Directory.GetFiles(path))
                    {
                        this.BuildScriptPathList(filePath, searchRecursively, scriptFilePaths);
                    }
                }
            }
            else if (File.Exists(path))
            {
                String fileName = Path.GetFileName(path);
                if ((fileName.Length >= ps1Suffix.Length && String.Equals(Path.GetExtension(path), ps1Suffix, StringComparison.OrdinalIgnoreCase)) ||
                    (fileName.Length >= psm1Suffix.Length && String.Equals(Path.GetExtension(path), psm1Suffix, StringComparison.OrdinalIgnoreCase)) ||
                    (fileName.Length >= psd1Suffix.Length && String.Equals(Path.GetExtension(path), psd1Suffix, StringComparison.OrdinalIgnoreCase)))
                {
                    scriptFilePaths.Add(path);
                }
                else if (Helper.Instance.IsHelpFile(path))
                {
                    scriptFilePaths.Add(path);
                }
            }
            else
            {
                this.outputWriter.WriteError(
                    new ErrorRecord(
                        new FileNotFoundException(),
                        string.Format(CultureInfo.CurrentCulture, Strings.FileNotFound, path),
                        ErrorCategory.InvalidArgument,
                        this));
            }
        }

#if !PSV3
        private bool TrySaveModules(ParseError[] errors, ScriptBlockAst scriptAst)
        {
            bool modulesSaved = false;
            if (moduleHandler == null || errors == null || errors.Length == 0)
            {
                return modulesSaved;
            }
            foreach (var error in errors.Where(IsModuleNotFoundError))
            {
                var moduleNames = moduleHandler.GetUnavailableModuleNameFromErrorExtent(error, scriptAst);
                if (moduleNames == null)
                {
                    continue;
                }
                foreach(var moduleName in moduleNames)
                {
                    this.outputWriter.WriteVerbose(
                        String.Format(
                            "Saving module {0} from PSGallery",
                            moduleName));
                    var moduleSaved = moduleHandler.TrySaveModule(moduleName);
                    if (!moduleSaved)
                    {
                        this.outputWriter.WriteVerbose(
                            String.Format(
                                "Cannot download {0} from PSGallery",
                                moduleName));
                    }
                    modulesSaved |= moduleSaved;
                }
            }
            return modulesSaved;
        }
#endif // !PSV3

        private IEnumerable<DiagnosticRecord> AnalyzeFile(string filePath)
        {
            ScriptBlockAst scriptAst = null;
            Token[] scriptTokens = null;
            ParseError[] errors = null;

            this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseFileMessage, filePath));

            //Parse the file
            if (File.Exists(filePath))
            {
                // processing for non help script
                if (!(Path.GetFileName(filePath).ToLower().StartsWith("about_") && Path.GetFileName(filePath).ToLower().EndsWith(".help.txt")))
                {
                    try
                    {
                        scriptAst = Parser.ParseFile(filePath, out scriptTokens, out errors);
                    }
                    catch (Exception e)
                    {
                        this.outputWriter.WriteWarning(e.ToString());
                        return null;
                    }
#if !PSV3
                    //try parsing again
                    if (TrySaveModules(errors, scriptAst))
                    {
                        scriptAst = Parser.ParseFile(filePath, out scriptTokens, out errors);
                    }
#endif //!PSV3
                    //Runspace.DefaultRunspace = oldDefault;
                    if (errors != null && errors.Length > 0)
                    {
                        foreach (ParseError error in errors)
                        {
                            string parseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParserErrorFormat, error.Extent.File, error.Message.TrimEnd('.'), error.Extent.StartLineNumber, error.Extent.StartColumnNumber);
                            this.outputWriter.WriteError(new ErrorRecord(new ParseException(parseErrorMessage), parseErrorMessage, ErrorCategory.ParserError, error.ErrorId));
                        }
                    }

                    if (errors != null && errors.Length > 10)
                    {
                        string manyParseErrorMessage = String.Format(CultureInfo.CurrentCulture, Strings.ParserErrorMessage, System.IO.Path.GetFileName(filePath));
                        this.outputWriter.WriteError(new ErrorRecord(new ParseException(manyParseErrorMessage), manyParseErrorMessage, ErrorCategory.ParserError, filePath));
                        return new List<DiagnosticRecord>();
                    }
                }
            }
            else
            {
                this.outputWriter.ThrowTerminatingError(new ErrorRecord(new FileNotFoundException(),
                    string.Format(CultureInfo.CurrentCulture, Strings.InvalidPath, filePath),
                    ErrorCategory.InvalidArgument, filePath));

                return null;
            }

            return this.AnalyzeSyntaxTree(scriptAst, scriptTokens, filePath);
        }

        private bool IsModuleNotFoundError(ParseError error)
        {
            return error.ErrorId != null
                && error.ErrorId.Equals("ModuleNotFoundDuringParse", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSeverityAllowed(IEnumerable<uint> allowedSeverities, IRule rule)
        {
            return severity == null
                || (allowedSeverities != null
                    && rule != null
                    && HasGetSeverity(rule)
                    && allowedSeverities.Contains((uint)rule.GetSeverity()));
        }

        private bool IsRuleEnabled(IRule rule)
        {
            var configurableRule = rule as ConfigurableScriptRule;
            if (configurableRule != null)
            {
                var propertyInfo = configurableRule.GetType().GetProperty("Enable");
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(configurableRule) as bool? ?? true;
                }
            }

            return true;
        }

        IEnumerable<uint> GetAllowedSeveritiesInInt()
        {
            return severity != null
                ? severity.Select(item => (uint)Enum.Parse(typeof(DiagnosticSeverity), item, true))
                : null;
        }

        bool HasMethod<T>(T obj, string methodName)
        {
            var type = obj.GetType();
            return type.GetMethod(methodName) != null;
        }

        bool HasGetSeverity<T>(T obj)
        {
            return HasMethod<T>(obj, "GetSeverity");
        }

        bool IsRuleAllowed(IRule rule)
        {
            IEnumerable<uint> allowedSeverities = GetAllowedSeveritiesInInt();
            bool includeRegexMatch = false;
            bool excludeRegexMatch = false;
            foreach (Regex include in includeRegexList)
            {
                if (include.IsMatch(rule.GetName()))
                {
                    includeRegexMatch = true;
                    break;
                }
            }

            foreach (Regex exclude in excludeRegexList)
            {
                if (exclude.IsMatch(rule.GetName()))
                {
                    excludeRegexMatch = true;
                    break;
                }
            }

            bool helpRule = String.Equals(rule.GetName(), "PSUseUTF8EncodingForHelpFile", StringComparison.OrdinalIgnoreCase);
            bool includeSeverity = IsSeverityAllowed(allowedSeverities, rule);

            return (includeRule == null || includeRegexMatch)
                    && (excludeRule == null || !excludeRegexMatch)
                    && IsSeverityAllowed(allowedSeverities, rule)
                    && IsRuleEnabled(rule);
        }

        /// <summary>
        /// Wrapper around the Helper.SuppressRule method
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="ruleSuppressions"></param>
        /// <param name="ruleDiagnosticRecords"></param>
        /// <returns>Returns a tuple of suppressed and diagnostic records</returns>
        private Tuple<List<SuppressedRecord>, List<DiagnosticRecord>> SuppressRule(
            string ruleName,
            Dictionary<string, List<RuleSuppression>> ruleSuppressions,
            List<DiagnosticRecord> ruleDiagnosticRecords)
        {
            List<ErrorRecord> suppressRuleErrors;
            var records = Helper.Instance.SuppressRule(
                ruleName,
                ruleSuppressions,
                ruleDiagnosticRecords,
                out suppressRuleErrors);
            foreach (var error in suppressRuleErrors)
            {
                this.outputWriter.WriteError(error);
            }
            return records;
        }

        /// <summary>
        /// Wrapper around SuppressRule method.
        ///
        /// This enables suppressing external rules (written in PowerShell).
        /// Since there can be an inconsistency between the rule name in
        /// diagnostic record and the actual rule (function) name, we use
        /// the diagnostic record rule name for suppression
        /// </summary>
        /// <param name="ruleSuppressions"></param>
        /// <param name="ruleDiagnosticRecord"></param>
        /// <returns>Returns a tuple of suppressed and diagnostic records</returns>
        private Tuple<List<SuppressedRecord>, List<DiagnosticRecord>> SuppressRule(
            Dictionary<string, List<RuleSuppression>> ruleSuppressions,
            DiagnosticRecord ruleDiagnosticRecord
            )
        {
            return SuppressRule(
                ruleDiagnosticRecord.RuleName,
                ruleSuppressions,
                new List<DiagnosticRecord> { ruleDiagnosticRecord });
        }

        /// <summary>
        /// Analyzes the syntax tree of a script file that has already been parsed.
        /// </summary>
        /// <param name="scriptAst">The ScriptBlockAst from the parsed script.</param>
        /// <param name="scriptTokens">The tokens found in the script.</param>
        /// <param name="filePath">The path to the file that was parsed.
        /// If AnalyzeSyntaxTree is called from an ast that we get from ParseInput, then this field will be String.Empty
        /// </param>
        /// <returns>An enumeration of DiagnosticRecords that were found by rules.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeSyntaxTree(
            ScriptBlockAst scriptAst,
            Token[] scriptTokens,
            string filePath)
        {
            Dictionary<string, List<RuleSuppression>> ruleSuppressions = new Dictionary<string,List<RuleSuppression>>();
            ConcurrentBag<DiagnosticRecord> diagnostics = new ConcurrentBag<DiagnosticRecord>();
            ConcurrentBag<SuppressedRecord> suppressed = new ConcurrentBag<SuppressedRecord>();
            BlockingCollection<List<object>> verboseOrErrors = new BlockingCollection<List<object>>();

            // Use a List of KVP rather than dictionary, since for a script containing inline functions with same signature, keys clash
            List<KeyValuePair<CommandInfo, IScriptExtent>> cmdInfoTable = new List<KeyValuePair<CommandInfo, IScriptExtent>>();
            bool filePathIsNullOrWhiteSpace = String.IsNullOrWhiteSpace(filePath);
            filePath = filePathIsNullOrWhiteSpace ? String.Empty : filePath;

            // check whether the script we are analyzing is a help file or not.
            // this step is not applicable for scriptdefinition, whose filepath is null
            bool helpFile = (scriptAst == null) && (!filePathIsNullOrWhiteSpace) && Helper.Instance.IsHelpFile(filePath);

            if (!helpFile)
            {
                ruleSuppressions = Helper.Instance.GetRuleSuppression(scriptAst);

                foreach (List<RuleSuppression> ruleSuppressionsList in ruleSuppressions.Values)
                {
                    foreach (RuleSuppression ruleSuppression in ruleSuppressionsList)
                    {
                        if (!String.IsNullOrWhiteSpace(ruleSuppression.Error))
                        {
                            this.outputWriter.WriteError(new ErrorRecord(new ArgumentException(ruleSuppression.Error), ruleSuppression.Error, ErrorCategory.InvalidArgument, ruleSuppression));
                        }
                    }
                }

#region Run VariableAnalysis
                try
                {
                    Helper.Instance.InitializeVariableAnalysis(scriptAst);
                }
                catch { }
#endregion

                Helper.Instance.Tokens = scriptTokens;
            }

#region Run ScriptRules
            //Trim down to the leaf element of the filePath and pass it to Diagnostic Record
            string fileName = filePathIsNullOrWhiteSpace ? String.Empty : System.IO.Path.GetFileName(filePath);
            if (this.ScriptRules != null)
            {
                var allowedRules = this.ScriptRules.Where(IsRuleAllowed);
                if (allowedRules.Any())
                {
                    var tasks = allowedRules.Select(scriptRule => Task.Factory.StartNew(() =>
                    {
                        bool helpRule = String.Equals(scriptRule.GetName(), "PSUseUTF8EncodingForHelpFile", StringComparison.OrdinalIgnoreCase);
                        List<object> result = new List<object>();
                        result.Add(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, scriptRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            if (helpRule && helpFile)
                            {
                                var records = scriptRule.AnalyzeScript(scriptAst, filePath);
                                foreach (var record in records)
                                {
                                    diagnostics.Add(record);
                                }
                            }
                            else if (!helpRule && !helpFile)
                            {
                                List<ErrorRecord> suppressRuleErrors;
                                var ruleRecords = scriptRule.AnalyzeScript(scriptAst, scriptAst.Extent.File).ToList();
                                var records = Helper.Instance.SuppressRule(
                                    scriptRule.GetName(),
                                    ruleSuppressions,
                                    ruleRecords,
                                    out suppressRuleErrors);
                                result.AddRange(suppressRuleErrors);
                                foreach (var record in records.Item2)
                                {
                                    diagnostics.Add(record);
                                }
                                foreach (var suppressedRec in records.Item1)
                                {
                                    suppressed.Add(suppressedRec);
                                }
                            }
                        }
                        catch (Exception scriptRuleException)
                        {
                            result.Add(new ErrorRecord(scriptRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, scriptAst.Extent.File));
                        }

                        verboseOrErrors.Add(result);
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
                            this.outputWriter.WriteVerbose(data[0] as string);
                            if (data.Count > 1)
                            {
                                for (int count = 1; count < data.Count; count++)
                                {
                                    this.outputWriter.WriteError(data[count] as ErrorRecord);
                                }
                            }
                        }
                    }
                }
            }

#endregion

#region Run Token Rules

            if (this.TokenRules != null && !helpFile)
            {
                foreach (ITokenRule tokenRule in this.TokenRules)
                {
                    if (IsRuleAllowed(tokenRule))
                    {
                        this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, tokenRule.GetName()));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            var ruleRecords = tokenRule.AnalyzeTokens(scriptTokens, filePath).ToList();
                            var records = SuppressRule(tokenRule.GetName(), ruleSuppressions, ruleRecords);
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
                            this.outputWriter.WriteError(new ErrorRecord(tokenRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, fileName));
                        }
                    }
                }
            }

#endregion

#region DSC Resource Rules
            if (this.DSCResourceRules != null && !helpFile)
            {
                // Invoke AnalyzeDSCClass only if the ast is a class based resource
                if (Helper.Instance.IsDscResourceClassBased(scriptAst))
                {
                    // Run DSC Class rule
                    foreach (IDSCResourceRule dscResourceRule in this.DSCResourceRules)
                    {
                        if (IsRuleAllowed(dscResourceRule))
                        {
                            this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                            try
                            {
#if PSV3
                                var errRecs = new List<ErrorRecord>();
                                var records = Helper.Instance.SuppressRule(
                                    dscResourceRule.GetName(),
                                    ruleSuppressions,
                                    null,
                                    out errRecs);
#else
                                var ruleRecords = dscResourceRule.AnalyzeDSCClass(scriptAst, filePath).ToList();
                                var records = SuppressRule(dscResourceRule.GetName(), ruleSuppressions, ruleRecords);
#endif
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
                                this.outputWriter.WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            }
                        }
                    }
                }

                // Check if the supplied artifact is indeed part of the DSC resource
                if (!filePathIsNullOrWhiteSpace && Helper.Instance.IsDscResourceModule(filePath))
                {
                    // Run all DSC Rules
                    foreach (IDSCResourceRule dscResourceRule in this.DSCResourceRules)
                    {
                        if (IsRuleAllowed(dscResourceRule))
                        {
                            this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, dscResourceRule.GetName()));

                            // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                            // We want the Engine to continue functioning even if one or more Rules throws an exception
                            try
                            {
                                var ruleRecords = dscResourceRule.AnalyzeDSCResource(scriptAst, filePath).ToList();
                                var records = SuppressRule(dscResourceRule.GetName(), ruleSuppressions, ruleRecords);
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
                                this.outputWriter.WriteError(new ErrorRecord(dscResourceRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, filePath));
                            }
                        }
                    }

                }
            }
#endregion

#region Run External Rules

            if (this.ExternalRules != null && !helpFile)
            {
                List<ExternalRule> exRules = new List<ExternalRule>();

                foreach (ExternalRule exRule in this.ExternalRules)
                {
                    if (IsRuleAllowed(exRule))
                    {
                        string ruleName = string.Format(CultureInfo.CurrentCulture, "{0}\\{1}", exRule.GetSourceName(), exRule.GetName());
                        this.outputWriter.WriteVerbose(string.Format(CultureInfo.CurrentCulture, Strings.VerboseRunningMessage, ruleName));

                        // Ensure that any unhandled errors from Rules are converted to non-terminating errors
                        // We want the Engine to continue functioning even if one or more Rules throws an exception
                        try
                        {
                            exRules.Add(exRule);
                        }
                        catch (Exception externalRuleException)
                        {
                            this.outputWriter.WriteError(new ErrorRecord(externalRuleException, Strings.RuleErrorMessage, ErrorCategory.InvalidOperation, fileName));
                        }
                    }
                }

                foreach (var ruleRecord in this.GetExternalRecord(scriptAst, scriptTokens, exRules.ToArray(), filePath))
                {
                    var records = SuppressRule(ruleSuppressions, ruleRecord);
                    foreach (var record in records.Item2)
                    {
                        diagnostics.Add(record);
                    }
                    foreach (var suppressedRec in records.Item1)
                    {
                        suppressed.Add(suppressedRec);
                    }
                }
            }

            #endregion

            // Need to reverse the concurrentbag to ensure that results are sorted in the increasing order of line numbers
            IEnumerable<DiagnosticRecord> diagnosticsList = diagnostics.Reverse();

            return this.suppressedOnly ?
                suppressed.OfType<DiagnosticRecord>() :
                diagnosticsList;
        }
    }
}
