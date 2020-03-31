// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;
using System.IO;

#if CoreCLR
using System.Runtime.InteropServices;
#endif

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    /// <summary>
    /// Collects compatibility information for PowerShell on the current platform.
    /// </summary>
    public class CompatibilityProfileCollector : IDisposable
    {
        /// <summary>
        /// Builds a compatibility profiler collector in a configurable way.
        /// </summary>
        public class Builder
        {
            private PowerShellDataCollector.Builder _pwshDataCollectorBuilder;

            private TypeDataCollector.Builder _typeDataColletorBuilder;

            /// <summary>
            /// Create a new builder instance.
            /// </summary>
            public Builder()
            {
                _pwshDataCollectorBuilder = new PowerShellDataCollector.Builder();
                _typeDataColletorBuilder = new TypeDataCollector.Builder();
            }

            /// <summary>
            /// Modules on paths starting with these prefixes will excluded from profile collection.
            /// </summary>
            public IReadOnlyCollection<string> ExcludedModulePathPrefixes
            {
                get => _pwshDataCollectorBuilder.ExcludedModulePathPrefixes;
                set
                {
                    _pwshDataCollectorBuilder.ExcludedModulePathPrefixes = value;
                }
            }

            /// <summary>
            /// .NET assemblies on paths starting with these prefixes will excluded from profile collection.
            /// </summary>
            public IReadOnlyCollection<string> ExcludedAssemblyPathPrefixes
            {
                get => _typeDataColletorBuilder.ExcludedAssemblyPathPrefixes;
                set
                {
                    _typeDataColletorBuilder.ExcludedAssemblyPathPrefixes = value;
                }
            }

            /// <summary>
            /// Build a new PowerShell compatibility profile collector around a PowerShell session.
            /// </summary>
            /// <param name="pwsh">The PowerShell wrapper to provide PowerShell functionality from.</param>
            /// <returns>A new compatibility profile collector for profiling with.</returns>
            public CompatibilityProfileCollector Build(SMA.PowerShell pwsh)
            {
                var platformInfoCollector = new PlatformInformationCollector(pwsh);

                return new CompatibilityProfileCollector(
                    pwsh,
                    platformInfoCollector,
                    _pwshDataCollectorBuilder.Build(pwsh, platformInfoCollector.PSVersion),
                    _typeDataColletorBuilder.Build(Path.GetDirectoryName(typeof(SMA.PowerShell).Assembly.Location)));
            }
        }

        // Increment the minor version if non-breaking additions have been made to the API
        // Increment the major version if breaking changes have been made to the API
        private static readonly Version s_currentProfileSchemaVersion = new Version(1, 2);

        private readonly PowerShellDataCollector _pwshDataCollector;

        private readonly TypeDataCollector _typeDataCollector;

        private readonly PlatformInformationCollector _platformInfoCollector;

        private readonly Func<ApplicationInfo, Version> _getApplicationVersion;

        private SMA.PowerShell _pwsh;

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

        /// <summary>
        /// Gets a full PowerShell compatibility profile from the current session.
        /// </summary>
        /// <param name="errors">Any errors encountered while collecting the profile. May be null.</param>
        /// <returns>A PowerShell compatibility profile for the current session.</returns>
        public CompatibilityProfileData GetCompatibilityData(out IEnumerable<Exception> errors)
        {
            return GetCompatibilityData(platformId: null, errors: out errors);
        }

        /// <summary>
        /// Get a PowerShell compatibility profile and names it using the platform ID given.
        /// If the ID is null, uses the canonical platform name.
        /// </summary>
        /// <param name="platformId">The platform ID to use for the profile.</param>
        /// <param name="errors">Errors encountered collecting the profile, if any. May be null.</param>
        /// <returns>The compatibility profile for the running PowerShell session.</returns>
        public CompatibilityProfileData GetCompatibilityData(string platformId, out IEnumerable<Exception> errors)
        {
            PlatformData platformData = _platformInfoCollector.GetPlatformData();

            return new CompatibilityProfileData()
            {
                ProfileSchemaVersion = s_currentProfileSchemaVersion,
                Id = platformId ?? PlatformNaming.GetPlatformName(platformData),
                Platform = platformData,
                Runtime = GetRuntimeData(out errors)
            };
        }

        /// <summary>
        /// Get PowerShell runtime compatibility data.
        /// </summary>
        /// <param name="errors">Any errors encountered during collection. May be null.</param>
        /// <returns>A runtime compatibility profile object.</returns>
        public RuntimeData GetRuntimeData(out IEnumerable<Exception> errors)
        {
            // Need to ensure modules are imported before types are collected
            JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>> modules = _pwshDataCollector.AssembleModulesData(_pwshDataCollector.GetModulesData(out IEnumerable<Exception> moduleErrors));
            Data.AvailableTypeData availableTypeData = _typeDataCollector.GetAvailableTypeData(out IEnumerable<CompatibilityAnalysisException> typeErrors);

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

        /// <summary>
        /// Gets common PowerShell feature information, such as common parameters and parameter aliases.
        /// </summary>
        /// <returns>An object containing common feature compatibility information.</returns>
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

        /// <summary>
        /// Gets compatibility information for native commands available in the current session.
        /// </summary>
        /// <returns>An enumeration of native commands available in the current session.</returns>
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

#if CoreCLR
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#else
                if (_platformInfoCollector.PSVersion.Major >= 5)
#endif
                {
                    commandData.Version = _getApplicationVersion(command);
                }

                yield return new KeyValuePair<string, NativeCommandData>(command.Name, commandData);
            }
        }

        /// <summary>
        /// Assembles the given enumeration of native command data into a lookup table.
        /// </summary>
        /// <param name="commands">The native command information to assemble.</param>
        /// <returns>A case-insensitive dictionary of all native commands.</returns>
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
                    _pwshDataCollector.Dispose();
                    _platformInfoCollector.Dispose();
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