// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    /// <summary>
    /// Collects information about modules, types and commands available in a PowerShell session.
    /// </summary>
    public class PowerShellDataCollector : IDisposable
    {
        /// <summary>
        /// Configures and builds a PowerShellDataCollector object.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Modules on paths underneath any of these will be excluded.
            /// </summary>
            public IReadOnlyCollection<string> ExcludedModulePathPrefixes { get; set; }

            /// <summary>
            /// Build a new PowerShellDataCollector with the given configuration.
            /// </summary>
            /// <param name="pwsh">The PowerShell session to collect data from.</param>
            /// <param name="psVersion">The version of PowerShell running.</param>
            /// <returns>The constructed PowerShell data collector object.</returns>
            public PowerShellDataCollector Build(SMA.PowerShell pwsh, PowerShellVersion psVersion)
            {
                return new PowerShellDataCollector(pwsh, psVersion, ExcludedModulePathPrefixes);
            }
        }

        private const string DEFAULT_DEFAULT_PARAMETER_SET = "__AllParameterSets";

        private const string CORE_MODULE_NAME = "Microsoft.PowerShell.Core";

        private const string THIS_MODULE_NAME = "PSCompatibilityCollector";

        private static readonly Regex s_typeDataRegex = new Regex("Error in TypeData \"([A-Za-z\\.]+)\"", RegexOptions.Compiled);

        private static readonly CmdletInfo s_gmoInfo = new CmdletInfo("Get-Module", typeof(GetModuleCommand));

        private static readonly CmdletInfo s_ipmoInfo = new CmdletInfo("Import-Module", typeof(ImportModuleCommand));

        private static readonly CmdletInfo s_rmoInfo = new CmdletInfo("Remove-Module", typeof(RemoveModuleCommand));

        private static readonly CmdletInfo s_gcmInfo = new CmdletInfo("Get-Command", typeof(GetCommandCommand));

        internal static CmdletInfo GcmInfo => s_gcmInfo;

        private readonly PowerShellVersion _psVersion;

        private readonly Lazy<ReadOnlySet<string>> _lazyCommonParameters;

        private readonly IReadOnlyCollection<string> _excludedModulePrefixes;

        private SMA.PowerShell _pwsh;

        private PowerShellDataCollector(
            SMA.PowerShell pwsh,
            PowerShellVersion psVersion,
            IReadOnlyCollection<string> excludedModulePrefixes)
        {
            _pwsh = pwsh;
            _psVersion = psVersion;
            _excludedModulePrefixes = excludedModulePrefixes;
            _lazyCommonParameters = new Lazy<ReadOnlySet<string>>(GetPowerShellCommonParameterNames);
        }

        internal ReadOnlySet<string> CommonParameterNames => _lazyCommonParameters.Value;

        /// <summary>
        /// Assemble module data objects into a lookup table.
        /// </summary>
        /// <param name="modules">An enumeration of module data objects to assemble</param>
        /// <returns>A case-insensitive dictionary of versioned module data objects.</returns>
        public JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>> AssembleModulesData(
            IEnumerable<Tuple<string, Version, ModuleData>> modules)
        {
            var moduleDict = new JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>>();
            foreach (Tuple<string, Version, ModuleData> module in modules)
            {
                if (moduleDict.TryGetValue(module.Item1, out JsonDictionary<Version, ModuleData> versionDict))
                {
                    if (!versionDict.ContainsKey(module.Item2))
                    {
                        versionDict[module.Item2] = module.Item3;
                    }
                    continue;
                }

                var newVersionDict = new JsonDictionary<Version, ModuleData>();
                newVersionDict.Add(module.Item2, module.Item3);
                moduleDict.Add(module.Item1, newVersionDict);
            }

            return moduleDict;
        }

        /// <summary>
        /// Get data objects for all modules on the system.
        /// </summary>
        /// <param name="errors">Any errors encountered while collecting information.</param>
        /// <returns>An enumeration of module data, with the name and version of the module as well.</returns>
        public IEnumerable<Tuple<string, Version, ModuleData>> GetModulesData(out IEnumerable<Exception> errors)
        {
            IEnumerable<PSModuleInfo> modules = _pwsh.AddCommand(s_gmoInfo)
                .AddParameter("ListAvailable")
                .InvokeAndClear<PSModuleInfo>();

            var moduleDatas = new List<Tuple<string, Version, ModuleData>>();

            // Add the core parts of the module
            moduleDatas.Add(GetCoreModuleData());

            var errs = new List<Exception>();
            foreach (PSModuleInfo module in modules)
            {
                if (string.IsNullOrEmpty(module.Name))
                {
                    continue;
                }

                if (module.Name.Equals(THIS_MODULE_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (_excludedModulePrefixes != null && IsExcludedPath(module.Path))
                {
                    continue;
                }

                Tuple<string, Version, ModuleData> moduleData = LoadAndGetModuleData(module, out Exception error);

                if (moduleData == null)
                {
                    if (error != null)
                    {
                        errs.Add(error);
                    }
                    continue;
                }

                moduleDatas.Add(moduleData);
            }

            errors = errs;
            return moduleDatas;
        }

        /// <summary>
        /// Loads and retrieves information about a given module.
        /// </summary>
        /// <param name="module">The module to load, given as a PSModuleInfo.</param>
        /// <param name="error">Any error encountered, if any.</param>
        /// <returns>The name, version and data object for the module, or null if data collection did not succeed.</returns>
        public Tuple<string, Version, ModuleData> LoadAndGetModuleData(PSModuleInfo module, out Exception error)
        {
            try
            {
                PSModuleInfo importedModule = _pwsh.AddCommand(s_ipmoInfo)
                    .AddParameter("ModuleInfo", module)
                    .AddParameter("PassThru")
                    .AddParameter("ErrorAction", "Stop")
                    .InvokeAndClear<PSModuleInfo>()
                    .FirstOrDefault();

                if (importedModule == null)
                {
                    error = null;
                    return null;
                }

                Tuple<string, Version, ModuleData> moduleData = GetSingleModuleData(importedModule);

                _pwsh.AddCommand(s_rmoInfo)
                    .AddParameter("ModuleInfo", importedModule)
                    .InvokeAndClear();

                error = null;
                return moduleData;
            }
            catch (RuntimeException e)
            {
                // A common problem is TypeData being hit with other modules, this overrides that
                if (e.ErrorRecord.FullyQualifiedErrorId.Equals("FormatXmlUpdateException,Microsoft.PowerShell.Commands.ImportModuleCommand")
                    || e.ErrorRecord.FullyQualifiedErrorId.Equals("ErrorsUpdatingTypes"))
                {
                   foreach (string typeDataName in GetTypeDataNamesFromErrorMessage(e.Message))
                   {
                       _pwsh.AddCommand("Remove-TypeData")
                            .AddParameter("TypeName", typeDataName)
                            .InvokeAndClear();
                   }
                }

                // Attempt to load the module in a new runspace instead
                try
                {
                    using (SMA.PowerShell fallbackPwsh = SMA.PowerShell.Create(RunspaceMode.NewRunspace))
                    {
                        PSModuleInfo importedModule = fallbackPwsh.AddCommand(s_ipmoInfo)
                            .AddParameter("Name", module.Path)
                            .AddParameter("PassThru")
                            .AddParameter("Force")
                            .AddParameter("ErrorAction", "Stop")
                            .InvokeAndClear<PSModuleInfo>()
                            .FirstOrDefault();

                        error = null;
                        return GetSingleModuleData(importedModule);
                    }
                }
                catch (Exception fallbackException)
                {
                    error = fallbackException;
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the module data for a single loaded module.
        /// Unloaded modules may only present partial data.
        /// </summary>
        /// <param name="module">The module to collect data from.</param>
        /// <returns>A data object describing the given module.</returns>
        public Tuple<string, Version, ModuleData> GetSingleModuleData(PSModuleInfo module)
        {
            var moduleData = new ModuleData()
            {
                Guid = module.Guid
            };

            if (module.ExportedAliases != null && module.ExportedAliases.Count > 0)
            {
                moduleData.Aliases = new JsonCaseInsensitiveStringDictionary<string>(module.ExportedAliases.Count);
                moduleData.Aliases.AddAll(GetAliasesData(module.ExportedAliases.Values));
            }

            if (module.ExportedCmdlets != null && module.ExportedCmdlets.Count > 0)
            {
                moduleData.Cmdlets = new JsonCaseInsensitiveStringDictionary<CmdletData>(module.ExportedCmdlets.Count);
                moduleData.Cmdlets.AddAll(GetCmdletsData(module.ExportedCmdlets.Values));
            }

            if (module.ExportedFunctions != null && module.ExportedFunctions.Count > 0)
            {
                moduleData.Functions = new JsonCaseInsensitiveStringDictionary<FunctionData>(module.ExportedCmdlets.Count);
                moduleData.Functions.AddAll(GetFunctionsData(module.ExportedFunctions.Values));
            }

            if (module.ExportedVariables != null && module.ExportedVariables.Count > 0)
            {
                moduleData.Variables = GetVariablesData(module.ExportedVariables.Values);
            }

            return new Tuple<string, Version, ModuleData>(module.Name, module.Version, moduleData);
        }

        /// <summary>
        /// Get a module data object for the Microsoft.PowerShell.Core pseudo-module.
        /// The version is given as the PowerShell version.
        /// </summary>
        /// <returns>The name, version and data of the core pseudo-module.</returns>
        public Tuple<string, Version, ModuleData> GetCoreModuleData()
        {
            var moduleData = new ModuleData();

            IEnumerable<CommandInfo> coreCommands = _pwsh.AddCommand(GcmInfo)
                .AddParameter("Type", CommandTypes.Alias | CommandTypes.Cmdlet | CommandTypes.Function)
                .InvokeAndClear<CommandInfo>()
                .Where(commandInfo => string.IsNullOrEmpty(commandInfo.ModuleName) || CORE_MODULE_NAME.Equals(commandInfo.ModuleName, StringComparison.OrdinalIgnoreCase));

            var cmdletData = new JsonCaseInsensitiveStringDictionary<CmdletData>();
            var functionData = new JsonCaseInsensitiveStringDictionary<FunctionData>();
            var aliases = new JsonCaseInsensitiveStringDictionary<string>();
            var aliasesToRequest = new List<string>();
            foreach (CommandInfo command in coreCommands)
            {
                switch (command)
                {
                    case CmdletInfo cmdlet:
                        try
                        {
                            cmdletData.Add(cmdlet.Name, GetSingleCmdletData(cmdlet));
                        }
                        catch (RuntimeException)
                        {
                            // If we can't load the cmdlet, we just move on
                        }
                        continue;

                    case FunctionInfo function:
                        try
                        {
                            functionData.Add(function.Name, GetSingleFunctionData(function));
                        }
                        catch (RuntimeException)
                        {
                            // Some functions have problems loading,
                            // which likely means PowerShell wouldn't be able to run them
                        }
                        continue;

                    case AliasInfo alias:
                        try
                        {
                            // Some aliases won't resolve unless specified specifically
                            if (alias.Definition == null)
                            {
                                aliasesToRequest.Add(alias.Name);
                                continue;
                            }

                            aliases.Add(alias.Name, alias.Definition);
                        }
                        catch (RuntimeException)
                        {
                            // Ignore aliases that have trouble loading
                        }
                        continue;

                    default:
                        throw new CompatibilityAnalysisException($"Command {command.Name} in core module is of unsupported type {command.CommandType}");
                }
            }

            moduleData.Cmdlets = cmdletData;
            moduleData.Functions = functionData;

            if (aliasesToRequest != null && aliasesToRequest.Count > 0)
            {
                IEnumerable<AliasInfo> resolvedAliases = _pwsh.AddCommand(GcmInfo)
                    .AddParameter("Name", aliasesToRequest)
                    .InvokeAndClear<AliasInfo>();

                foreach (AliasInfo resolvedAlias in resolvedAliases)
                {
                    if (resolvedAlias?.Definition == null)
                    {
                        continue;
                    }

                    aliases[resolvedAlias.Name] = resolvedAlias.Definition;
                }
            }

            // Get default variables and core aliases out of a fresh runspace
            using (SMA.PowerShell freshPwsh = SMA.PowerShell.Create(RunspaceMode.NewRunspace))
            {
                Collection<PSObject> varsAndAliases = freshPwsh.AddCommand("Get-ChildItem")
                    .AddParameter("Path", "variable:")
                    .InvokeAndClear();

                var variables = new List<string>();

                foreach (PSObject returnedObject in varsAndAliases)
                {
                    switch (returnedObject.BaseObject)
                    {
                        case PSVariable variable:
                            variables.Add(variable.Name);
                            continue;

                        // Skip over other objects we get back, since there's no reason to throw
                    }
                }

                moduleData.Variables = variables.ToArray();
                moduleData.Aliases = aliases;
            }

            Version psVersion = _psVersion.PreReleaseLabel != null
                ? new Version(_psVersion.Major, _psVersion.Minor, _psVersion.Build)
                : (Version)_psVersion;

            return new Tuple<string, Version, ModuleData>(CORE_MODULE_NAME, psVersion, moduleData);
        }

        /// <summary>
        /// Get an enumeration of alias data objects from a collection of aliases.
        /// </summary>
        /// <param name="aliases">PowerShell aliases to get data objects for.</param>
        /// <returns>An enumeration of data objects describing the given aliases.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetAliasesData(IEnumerable<AliasInfo> aliases)
        {
            foreach (AliasInfo alias in aliases)
            {
                yield return new KeyValuePair<string, string>(alias.Name, GetSingleAliasData(alias));
            }
        }

        /// <summary>
        /// Get the required data for a single PowerShell alias.
        /// </summary>
        /// <param name="alias">The alias to get data for.</param>
        /// <returns>The data describing the alias.</returns>
        public string GetSingleAliasData(AliasInfo alias)
        {
            return alias.Definition;
        }

        /// <summary>
        /// Get data objects describing the given cmdlets.
        /// </summary>
        /// <param name="cmdlets">The cmdlets to get data description objects for.</param>
        /// <returns>An enumeration of data objects about the given cmdlets.</returns>
        public IEnumerable<KeyValuePair<string, CmdletData>> GetCmdletsData(IEnumerable<CmdletInfo> cmdlets)
        {
            foreach (CmdletInfo cmdlet in cmdlets)
            {
                yield return new KeyValuePair<string, CmdletData>(cmdlet.Name, GetSingleCmdletData(cmdlet));
            }
        }

        /// <summary>
        /// Get a data object describing the given cmdlet.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to get a data object for.</param>
        /// <returns>A data object describing the given cmdlet.</returns>
        public CmdletData GetSingleCmdletData(CmdletInfo cmdlet)
        {
            var cmdletData = new CmdletData();

            cmdletData.DefaultParameterSet = GetDefaultParameterSet(cmdlet.DefaultParameterSet);

            cmdletData.OutputType = GetOutputType(cmdlet.OutputType);

            cmdletData.ParameterSets = GetParameterSets(cmdlet.ParameterSets);

            AssembleParameters(
                cmdlet.Parameters,
                out JsonCaseInsensitiveStringDictionary<ParameterData> parameters,
                out JsonCaseInsensitiveStringDictionary<string> parameterAliases,
                isCmdletBinding: true);

            cmdletData.Parameters = parameters;
            cmdletData.ParameterAliases = parameterAliases;

            return cmdletData;
        }

        /// <summary>
        /// Get a data object describing the parameter given by the given metadata object.
        /// </summary>
        /// <param name="parameter">The metadata of the parameter to describe.</param>
        /// <returns>A data object describing the given parameter.</returns>
        public ParameterData GetSingleParameterData(ParameterMetadata parameter)
        {
            var parameterData = new ParameterData()
            {
                Dynamic = parameter.IsDynamic
            };

            if (parameter.ParameterType != null)
            {
                parameterData.Type = TypeNaming.GetFullTypeName(parameter.ParameterType);
            }

            if (parameter.ParameterSets != null && parameter.ParameterSets.Count > 0)
            {
                parameterData.ParameterSets = new JsonCaseInsensitiveStringDictionary<ParameterSetData>();
                foreach (KeyValuePair<string, ParameterSetMetadata> parameterSet in parameter.ParameterSets)
                {
                    parameterData.ParameterSets[parameterSet.Key] = GetSingleParameterSetData(parameterSet.Value);
                }
            }

            return parameterData;
        }

        /// <summary>
        /// Get a data object describing a parameter set.
        /// </summary>
        /// <param name="parameterSet">The metadata object describing the parameter set to collect data on.</param>
        /// <returns>A compatibility data object describing the parameter set given.</returns>
        public ParameterSetData GetSingleParameterSetData(ParameterSetMetadata parameterSet)
        {
            var parameterSetData = new ParameterSetData()
            {
                Position = parameterSet.Position
            };

            var parameterSetFlags = new List<ParameterSetFlag>();

            if (parameterSet.IsMandatory)
            { 
                parameterSetFlags.Add(ParameterSetFlag.Mandatory);
            }

            if (parameterSet.ValueFromPipeline)
            {
                parameterSetFlags.Add(ParameterSetFlag.ValueFromPipeline);
            }

            if (parameterSet.ValueFromPipelineByPropertyName)
            {
                parameterSetFlags.Add(ParameterSetFlag.ValueFromPipelineByPropertyName);
            }

            if (parameterSet.ValueFromRemainingArguments)
            {
                parameterSetFlags.Add(ParameterSetFlag.ValueFromRemainingArguments);
            }

            if (parameterSetFlags.Count > 0)
            {
                parameterSetData.Flags = parameterSetFlags.ToArray();
            }

            return parameterSetData;
        }

        /// <summary>
        /// Get data objects describing the given functions.
        /// </summary>
        /// <param name="functions">The function info objects to get data objects for.</param>
        /// <returns>An enumeration of function data objects describing the functions given.</returns>
        public IEnumerable<KeyValuePair<string, FunctionData>> GetFunctionsData(IEnumerable<FunctionInfo> functions)
        {
            foreach (FunctionInfo function in functions)
            {
                yield return new KeyValuePair<string, FunctionData>(function.Name, GetSingleFunctionData(function));
            }
        }

        /// <summary>
        /// Get a data object describing the function given.
        /// </summary>
        /// <param name="function">A function info object describing a PowerShell function.</param>
        /// <returns>A function data object describing the given function.</returns>
        public FunctionData GetSingleFunctionData(FunctionInfo function)
        {
            var functionData = new FunctionData()
            {
                CmdletBinding = function.CmdletBinding
            };

            try
            {
                functionData.DefaultParameterSet = GetDefaultParameterSet(function.DefaultParameterSet);
                functionData.OutputType = GetOutputType(function.OutputType);
                functionData.ParameterSets = GetParameterSets(function.ParameterSets);
                AssembleParameters(
                    function.Parameters,
                    out JsonCaseInsensitiveStringDictionary<ParameterData> parameters,
                    out JsonCaseInsensitiveStringDictionary<string> parameterAliases,
                    isCmdletBinding: function.CmdletBinding);

                functionData.Parameters = parameters;
                functionData.ParameterAliases = parameterAliases;
            }
            catch (RuntimeException)
            {
                // This can fail when PowerShell can't resolve a type. So we just leave the field null
            }

            return functionData;
        }

        /// <summary>
        /// Get the names of the variables in the given list.
        /// </summary>
        /// <param name="variables">Variables to collect information on.</param>
        /// <returns>An array of the names of the variables.</returns>
        public string[] GetVariablesData(IEnumerable<PSVariable> variables)
        {
            var variableData = new string[variables.Count()];
            int i = 0;
            foreach (PSVariable variable in variables)
            {
                variableData[i] = variable.Name;
                i++;
            }

            return variableData;
        }

        private string GetDefaultParameterSet(string defaultParameterSet)
        {
            if (defaultParameterSet == null
                || string.Equals(defaultParameterSet, DEFAULT_DEFAULT_PARAMETER_SET))
            {
                return null;
            }

            return defaultParameterSet;
        }

        private string[] GetOutputType(IReadOnlyList<PSTypeName> outputType)
        {
            if (outputType == null || outputType.Count <= 0)
            {
                return null;
            }

            var outputTypeData = new string[outputType.Count];
            for (int i = 0; i < outputTypeData.Length; i++)
            {
                outputTypeData[i] = outputType[i].Type != null
                    ? TypeNaming.GetFullTypeName(outputType[i].Type)
                    : outputType[i].Name;
            }

            return outputTypeData;
        }

        private string[] GetParameterSets(IReadOnlyList<CommandParameterSetInfo> parameterSets)
        {
            if (parameterSets == null || parameterSets.Count <= 0)
            {
                return null;
            }

            var parameterSetData = new string[parameterSets.Count];
            for (int i = 0; i < parameterSetData.Length; i++)
            {
                parameterSetData[i] = parameterSets[i].Name;
            }

            return parameterSetData;
        }

        private void AssembleParameters(
            IReadOnlyDictionary<string, ParameterMetadata> parameters,
            out JsonCaseInsensitiveStringDictionary<ParameterData> parameterData,
            out JsonCaseInsensitiveStringDictionary<string> parameterAliasData,
            bool isCmdletBinding)
        {
            if (parameters == null || parameters.Count == 0)
            {
                parameterData = null;
                parameterAliasData = null;
                return;
            }

            parameterData = new JsonCaseInsensitiveStringDictionary<ParameterData>();
            parameterAliasData = null;

            foreach (KeyValuePair<string, ParameterMetadata> parameter in parameters)
            {
                if (isCmdletBinding && CommonParameterNames.Contains(parameter.Key))
                {
                    continue;
                }

                parameterData[parameter.Key] = GetSingleParameterData(parameter.Value);

                if (parameter.Value.Aliases != null && parameter.Value.Aliases.Count > 0)
                {
                    if (parameterAliasData == null)
                    {
                        parameterAliasData = new JsonCaseInsensitiveStringDictionary<string>();
                    }

                    foreach (string alias in parameter.Value.Aliases)
                    {
                        parameterAliasData[alias] = parameter.Key;
                    }
                }
            }

            if (parameterData.Count == 0)
            {
                parameterData = null;
            }

            if (parameterAliasData != null && parameterAliasData.Count == 0)
            {
                parameterAliasData = null;
            }
        }

        private bool IsExcludedPath(string path)
        {
            foreach (string excludedPathPrefix in _excludedModulePrefixes)
            {
#if CoreCLR
                StringComparison stringComparisonType = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;
#else
                StringComparison stringComparisonType = StringComparison.OrdinalIgnoreCase;
#endif
                if (path.StartsWith(excludedPathPrefix, stringComparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        private static ReadOnlySet<string> GetPowerShellCommonParameterNames()
        {
            const BindingFlags propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var set = new List<string>();
            foreach (PropertyInfo property in typeof(CommonParameters).GetProperties(propertyBindingFlags))
            {
                set.Add(property.Name);
            }
            return new ReadOnlySet<string>(set, StringComparer.OrdinalIgnoreCase);
        }

        private static string[] GetTypeDataNamesFromErrorMessage(string errorMessage)
        {
            var typeDataNames = new List<string>();
            foreach (Match match in s_typeDataRegex.Matches(errorMessage))
            {
                typeDataNames.Add(match.Groups[1].Value);
            }
            return typeDataNames.ToArray();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pwsh.Dispose();
                }

                _pwsh = null;
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}