using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Data.Modules;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    public class PowerShellDataCollector : IDisposable
    {
        private const string DEFAULT_DEFAULT_PARAMETER_SET = "__AllParameterSets";

        private SMA.PowerShell _pwsh;

        public PowerShellDataCollector(SMA.PowerShell pwsh)
        {
            _pwsh = pwsh;
        }

        public IEnumerable<KeyValuePair<string, ModuleData>> GetModulesData()
        {
            IEnumerable<PSModuleInfo> modules = _pwsh.AddCommand("Get-Module")
                .AddParameter("ListAvailable")
                .Invoke<PSModuleInfo>();

            foreach (PSModuleInfo module in modules)
            {
                PSModuleInfo importedModule = _pwsh.AddCommand("Import-Module")
                    .AddArgument(module)
                    .AddParameter("PassThru")
                    .Invoke<PSModuleInfo>()
                    .FirstOrDefault();

                yield return GetSingleModuleData(importedModule);

                _pwsh.AddCommand("Remove-Module")
                    .AddArgument(importedModule)
                    .Invoke();
            }
        }

        public KeyValuePair<string, ModuleData> GetSingleModuleData(PSModuleInfo module)
        {
            var moduleData = new ModuleData()
            {
                Guid = module.Guid
            };

            if (module.ExportedAliases != null && module.ExportedAliases.Count > 0)
            {
                moduleData.Aliases = new JsonCaseInsensitiveStringDictionary<string>(module.ExportedAliases.Count);
                moduleData.Aliases.AddAll(GetAliasesData(module.ExportedAliases));
            }

            if (module.ExportedCmdlets != null && module.ExportedCmdlets.Count > 0)
            {
                moduleData.Cmdlets = new JsonCaseInsensitiveStringDictionary<CmdletData>(module.ExportedCmdlets.Count);
                moduleData.Cmdlets.AddAll(GetCmdletsData(module.ExportedCmdlets));
            }

            if (module.ExportedFunctions != null && module.ExportedFunctions.Count > 0)
            {
                moduleData.Functions = new JsonCaseInsensitiveStringDictionary<FunctionData>(module.ExportedCmdlets.Count);
                moduleData.Functions.AddAll(GetFunctionsData(module.ExportedFunctions));
            }

            if (module.ExportedVariables != null && module.ExportedVariables.Count > 0)
            {
                moduleData.Variables = GetVariablesData(module.ExportedVariables);
            }

            return new KeyValuePair<string, ModuleData>(module.Name, moduleData);
        }

        public IEnumerable<KeyValuePair<string, string>> GetAliasesData(IReadOnlyDictionary<string, AliasInfo> aliases)
        {
            foreach (KeyValuePair<string, AliasInfo> alias in aliases)
            {
                yield return new KeyValuePair<string, string>(alias.Key, GetSingleAliasData(alias.Value));
            }
        }

        public string GetSingleAliasData(AliasInfo alias)
        {
            return alias.Definition;
        }

        public IEnumerable<KeyValuePair<string, CmdletData>> GetCmdletsData(IReadOnlyDictionary<string, CmdletInfo> cmdlets)
        {
            foreach (KeyValuePair<string, CmdletInfo> cmdlet in cmdlets)
            {
                yield return new KeyValuePair<string, CmdletData>(cmdlet.Key, GetSingleCmdletData(cmdlet.Value));
            }
        }

        public CmdletData GetSingleCmdletData(CmdletInfo cmdlet)
        {
            var cmdletData = new CmdletData();

            cmdletData.DefaultParameterSet = GetDefaultParameterSet(cmdlet.DefaultParameterSet);

            cmdletData.OutputType = GetOutputType(cmdlet.OutputType);

            cmdletData.ParameterSets = GetParameterSets(cmdlet.ParameterSets);

            AssembleParameters(
                cmdlet.Parameters,
                out JsonCaseInsensitiveStringDictionary<ParameterData> parameters,
                out JsonCaseInsensitiveStringDictionary<string> parameterAliases);

            cmdletData.Parameters = parameters;
            cmdletData.ParameterAliases = parameterAliases;

            return cmdletData;
        }

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

        public IEnumerable<KeyValuePair<string, FunctionData>> GetFunctionsData(IReadOnlyDictionary<string, FunctionInfo> functions)
        {
            foreach (KeyValuePair<string, FunctionInfo> function in functions)
            {
                yield return new KeyValuePair<string, FunctionData>(function.Key, GetSingleFunctionData(function.Value));
            }
        }

        public FunctionData GetSingleFunctionData(FunctionInfo function)
        {
            var functionData = new FunctionData()
            {
                CmdletBinding = function.CmdletBinding
            };

            functionData.DefaultParameterSet = GetDefaultParameterSet(function.DefaultParameterSet);

            functionData.OutputType = GetOutputType(function.OutputType);

            functionData.ParameterSets = GetParameterSets(function.ParameterSets);

            AssembleParameters(
                function.Parameters,
                out JsonCaseInsensitiveStringDictionary<ParameterData> parameters,
                out JsonCaseInsensitiveStringDictionary<string> parameterAliases);

            functionData.Parameters = parameters;
            functionData.ParameterAliases = parameterAliases;

            return functionData;
        }

        public string[] GetVariablesData(IReadOnlyDictionary<string, PSVariable> variables)
        {
            var variableData = new string[variables.Count];
            int i = 0;
            foreach (string variable in variables.Keys)
            {
                variableData[i] = variable;
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
            out JsonCaseInsensitiveStringDictionary<string> parameterAliasData)
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