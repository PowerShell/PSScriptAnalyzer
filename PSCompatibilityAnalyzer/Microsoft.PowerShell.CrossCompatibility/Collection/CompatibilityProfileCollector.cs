using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Data.Modules;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    public class CompatibilityProfileCollector : IDisposable
    {
        public class Builder
        {
            private PowerShellDataCollector.Builder _pwshDataCollectorBuilder;

            private TypeDataCollector.Builder _typeDataColletorBuilder;

            public Builder()
            {
                _pwshDataCollectorBuilder = new PowerShellDataCollector.Builder();
                _typeDataColletorBuilder = new TypeDataCollector.Builder();
            }

            public Builder ExcludedModulePathPrefixes(IReadOnlyCollection<string> modulePrefixes)
            {
                _pwshDataCollectorBuilder.ExcludedModulePathPrefixes(modulePrefixes);
                return this;
            }

            public Builder ExcludeAssemblyPathPrefixes(IReadOnlyCollection<string> assemblyPrefixes)
            {
                _typeDataColletorBuilder.ExcludedAssemblyPathPrefixes(assemblyPrefixes);
                return this;
            }

            public CompatibilityProfileCollector Build(SMA.PowerShell pwsh)
            {
                var platformInfoCollector = new PlatformInformationCollector(pwsh);

                return new CompatibilityProfileCollector(
                    pwsh,
                    platformInfoCollector,
                    _pwshDataCollectorBuilder.Build(pwsh, platformInfoCollector.PSVersion),
                    _typeDataColletorBuilder.Build());
            }
        }

        private SMA.PowerShell _pwsh;

        private readonly PowerShellDataCollector _pwshDataCollector;

        private readonly TypeDataCollector _typeDataCollector;

        private readonly PlatformInformationCollector _platformInfoCollector;

        private readonly Func<ApplicationInfo, Version> _getApplicationVersion;

        private CompatibilityProfileCollector(
            SMA.PowerShell pwsh,
            PlatformInformationCollector platformInfoCollector,
            PowerShellDataCollector pwshDataCollector,
            TypeDataCollector typeDataCollector)
        {
            _pwsh = pwsh;
            _platformInfoCollector = platformInfoCollector;
            _pwshDataCollector = pwshDataCollector;
            _typeDataCollector = typeDataCollector;

            if (_platformInfoCollector.PSVersion.Major >= 5)
            {
                _getApplicationVersion = GetApplicationVersionGetter();
            }
        }

        public CompatibilityProfileData GetCompatibilityData(out IEnumerable<Exception> errors)
        {
            return GetCompatibilityData(platformId: null, errors: out errors);
        }

        public CompatibilityProfileData GetCompatibilityData(string platformId, out IEnumerable<Exception> errors)
        {
            PlatformData platformData = _platformInfoCollector.GetPlatformData();

            return new CompatibilityProfileData()
            {
                Id = platformId ?? PlatformNaming.GetPlatformName(platformData),
                Platform = platformData,
                Runtime = GetRuntimeData(out errors)
            };
        }

        public RuntimeData GetRuntimeData(out IEnumerable<Exception> errors)
        {
            // Need to ensure modules are imported before types are collected
            JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>> modules = _pwshDataCollector.AssembleModulesData(_pwshDataCollector.GetModulesData(out IEnumerable<Exception> moduleErrors));
            Data.Types.AvailableTypeData availableTypeData = _typeDataCollector.GetAvailableTypeData(out IEnumerable<CompatibilityAnalysisException> typeErrors);

            var runtimeData = new RuntimeData()
            {
                Types = availableTypeData,
                Common = GetCommonPowerShellData(),
                NativeCommands = AssembleNativeCommands(GetNativeCommandData()),
                Modules = modules,
            };

            var errs = new List<Exception>();
            errs.AddRange(typeErrors);
            errs.AddRange(moduleErrors);
            errors = errs;

            return runtimeData;
        }

        public CommonPowerShellData GetCommonPowerShellData()
        {
            CmdletInfo gcmInfo = _pwsh.AddCommand(PowerShellDataCollector.GcmInfo)
                .AddParameter("Name", "Get-Command")
                .InvokeAndClear<CmdletInfo>()
                .FirstOrDefault();

            var commonParameters = new JsonCaseInsensitiveStringDictionary<ParameterData>();
            var commonParameterAliases = new JsonCaseInsensitiveStringDictionary<string>();
            foreach (string commonParameterName in _pwshDataCollector.CommonParameterNames)
            {
                if (gcmInfo.Parameters.TryGetValue(commonParameterName, out ParameterMetadata parameter))
                {
                    commonParameters.Add(commonParameterName, _pwshDataCollector.GetSingleParameterData(parameter));
                    foreach (string alias in parameter.Aliases)
                    {
                        commonParameterAliases.Add(alias, commonParameterName);
                    }
                }
            }

            return new CommonPowerShellData()
            {
                Parameters = commonParameters,
                ParameterAliases = commonParameterAliases,
            };
        }

        public IEnumerable<KeyValuePair<string, NativeCommandData>> GetNativeCommandData()
        {
            IEnumerable<ApplicationInfo> commands = _pwsh.AddCommand("Get-Command")
                .AddParameter("Type", "Application")
                .InvokeAndClear<ApplicationInfo>();

            foreach (ApplicationInfo command in commands)
            {
                var commandData = new NativeCommandData()
                {
                    Path = command.Path
                };

                if (_platformInfoCollector.PSVersion.Major >= 5)
                {
                    commandData.Version = _getApplicationVersion(command);
                }

                yield return new KeyValuePair<string, NativeCommandData>(command.Name, commandData);
            }
        }

        public JsonCaseInsensitiveStringDictionary<NativeCommandData[]> AssembleNativeCommands(
            IEnumerable<KeyValuePair<string, NativeCommandData>> commands)
        {
            var commandDict = new JsonCaseInsensitiveStringDictionary<NativeCommandData[]>();
            foreach (KeyValuePair<string, NativeCommandData> command in commands)
            {
                if (!commandDict.TryGetValue(command.Key, out NativeCommandData[] existingEntries))
                {
                    commandDict.Add(command.Key, new NativeCommandData[] { command.Value });
                    continue;
                }

                // We bank on there being few duplicate commands, so just copy the whole array each time
                var newCommandArray = new NativeCommandData[existingEntries.Length + 1];
                existingEntries.CopyTo(newCommandArray, 0);
                newCommandArray[existingEntries.Length] = command.Value;
                commandDict[command.Key] = newCommandArray;
            }

            return commandDict;
        }

        private static Func<ApplicationInfo, Version> GetApplicationVersionGetter()
        {
            MethodInfo applicationVersionGetter = typeof(ApplicationInfo).GetMethod("get_Version");

            return (Func<ApplicationInfo, Version>)Delegate.CreateDelegate(typeof(Func<ApplicationInfo, Version>), null, applicationVersionGetter);
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
                    _platformInfoCollector.Dispose();
                    _pwshDataCollector.Dispose();
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