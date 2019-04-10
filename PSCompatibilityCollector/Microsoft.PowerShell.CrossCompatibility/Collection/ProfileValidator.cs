// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.PowerShell.CrossCompatibility.Query;
using System.Linq;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    /// <summary>
    /// Object that takes a PowerShell compatibility profile and validates it.
    /// </summary>
    public abstract class ProfileValidator
    {
        /// <summary>
        /// Check if a PowerShell compatibility profile is valid.
        /// </summary>
        /// <param name="profileData">The compatibility profile to check.</param>
        /// <param name="validationErrors">All errors encountered with the validation. May be null when true is returned.</param>
        /// <returns>True if the profile is valid, false otherwise.</returns>
        public abstract bool IsProfileValid(Data.CompatibilityProfileData profileData, out IEnumerable<Exception> validationErrors);

        /// <summary>
        /// Resets the state of the validator so it is ready for another validation.
        /// May be implemented as a no-op with stateless validators.
        /// </summary>
        public abstract void Reset();
    }

    /// <summary>
    /// A simple profile validator that just checks that common keys and values are present.
    /// </summary>
    internal class QuickCheckValidator : ProfileValidator
    {
        private static IReadOnlyCollection<string> s_commonParameters = new []
        {
            "ErrorVariable",
            "WarningAction",
            "Verbose"
        };

        private static IReadOnlyCollection<string> s_commonParameterAliases = new []
        {
            "ev",
            "wa",
            "vb"
        };

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_modules = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            { 
                "Microsoft.PowerShell.Core", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Get-Module", new [] { "Name", "ListAvailable" } },
                    { "Start-Job", new [] { "ScriptBlock", "FilePath" } },
                    { "Where-Object", new [] { "FilterScript", "Property" } },
                }
            },
            {
                "Microsoft.PowerShell.Management", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Get-Process", new [] { "Name", "Id", "InputObject" } },
                    { "Test-Path", new [] { "Path", "LiteralPath" } },
                    { "Get-ChildItem", new [] { "Path", "LiteralPath" } },
                    { "New-Item", new [] { "Path", "Name", "Value" }},
                }
            },
            {
                "Microsoft.PowerShell.Utility", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "New-Object", new [] { "TypeName", "ArgumentList" }},
                    { "Write-Host", new [] { "Object", "NoNewline" }},
                    { "Out-File", new [] { "FilePath", "Encoding", "Append", "Force" }},
                    { "Invoke-Expression", new [] { "Command" }}
                }
            }
        };
        
        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_ps4Modules = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            {
                "PowerShellGet", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Install-Module", new [] { "Name", "Scope" } }
                }
            }
        };

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_ps5Modules = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            {
                "PSReadLine", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Set-PSReadLineKeyHandler", new [] { "Chord", "ScriptBlock" } },
                    { "Set-PSReadLineOption", new [] { "EditMode", "ContinuationPrompt" }}
                }
            }
        };

        private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> s_aliases = new Dictionary<string, IReadOnlyCollection<string>>()
        {
            { "Microsoft.PowerShell.Core", new [] { "?", "%", "select", "fl" , "iwr" } },
            { "Microsoft.PowerShell.Management", new [] { "stz", "gtz" } },
        };

        private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> s_ps5Aliases = new Dictionary<string, IReadOnlyCollection<string>>()
        {
            { "Microsoft.PowerShell.Utility", new [] { "fhx" } },
        };

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_types = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            {
                "System.Management.Automation", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "System.Management.Automation", new [] { "AliasInfo", "PSCmdlet", "PSModuleInfo", "SwitchParameter", "ProgressRecord" } },
                    { "System.Management.Automation.Language", new [] { "Parser", "AstVisitor", "ITypeName", "Token", "Ast" } },
                    { "Microsoft.PowerShell", new [] { "ExecutionPolicy" }},
                    { "Microsoft.PowerShell.Commands", new [] { "OutHostCommand", "GetCommandCommand", "GetModuleCommand", "InvokeCommandCommand", "ModuleCmdletBase" } }
                }
            },
            {
                "Microsoft.PowerShell.Commands.Utility", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Microsoft.PowerShell.Commands", new [] { "GetDateCommand", "NewObjectCommand", "SelectObjectCommand", "WriteOutputCommand", "GroupInfo", "GetRandomCommand" } }
                }
            },
            {
                "Microsoft.PowerShell.Commands.Management", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "Microsoft.PowerShell.Commands", new [] { "GetContentCommand", "CopyItemCommand", "TestPathCommand", "GetProcessCommand", "SetLocationCommand", "WriteContentLocationCommandBase" } }
                }
            }
        };

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_coreTypes = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            {
                "System.Private.CoreLib", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "System", new [] { "Object", "String", "Array", "Type" } },
                    { "System.Reflection", new [] { "Assembly", "BindingFlags", "FieldAttributes" } },
                    { "System.Collections.Generic", new [] { "Dictionary`2", "IComparer`1", "List`1", "IReadOnlyList`1" } }
                }
            },
            {
                "System.Collections", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "System.Collections", new [] { "BitArray" } },
                    { "System.Collections.Generic", new [] { "Queue`1", "Stack`1", "HashSet`1" } }
                }
            }
        };

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> s_fxTypes = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>>()
        {
            {
                "mscorlib", new Dictionary<string, IReadOnlyCollection<string>>()
                {
                    { "System", new [] { "Object", "String", "Array", "Type" } },
                    { "System.Reflection", new [] { "Assembly", "BindingFlags", "FieldAttributes" } },
                    { "System.Collections.Generic", new [] { "Dictionary`2", "IComparer`1", "List`1", "IReadOnlyList`1" } },
                    { "System.Collections", new [] { "BitArray" } }
                }
            }
        };

        private static IReadOnlyDictionary<string, string> s_typeAccelerators = new Dictionary<string, string>()
        {
            { "type", "System.Type" },
            { "psobject", "System.Management.Automation.PSObject" },
            { "pscustomobject", "System.Management.Automation.PSObject" },
            { "regex", "System.Text.RegularExpressions.Regex" },
            { "xml", "System.Xml.XmlDocument" },
            { "hashtable", "System.Collections.Hashtable" },
        };

        private ValidationErrorAccumulator _errAcc;

        private int _psVersionMajor;

        private Data.DotnetRuntime _dotNetEdition;

        /// <summary>
        /// Create a new quick check validator.
        /// </summary>
        public QuickCheckValidator()
        {
            _errAcc = new ValidationErrorAccumulator();
            _psVersionMajor = -1;
            _dotNetEdition = Data.DotnetRuntime.Other;
        }

        /// <inheritdoc/>
        public override bool IsProfileValid(Data.CompatibilityProfileData profileData, out IEnumerable<Exception> errors)
        {
            CompatibilityProfileData queryableProfile;
            try
            {
                queryableProfile = new CompatibilityProfileData(profileData);
            }
            catch (Exception e)
            {
                errors = new [] { e };
                return false;
            }

            try
            {
                _psVersionMajor = queryableProfile.Platform.PowerShell.Version.Major;
                ValidatePlatformData(queryableProfile.Id, queryableProfile.Platform);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }

            try
            {
                ValidateRuntimeData(queryableProfile.Runtime);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }

            if (_errAcc.HasErrors())
            {
                errors = _errAcc.GetErrors();
                return false;
            }

            errors = null;
            return true;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            _errAcc.Clear();
            _psVersionMajor = -1;
            _dotNetEdition = Data.DotnetRuntime.Other;
        }

        private void ValidatePlatformData(string platformId, PlatformData platformData)
        {
            if (string.IsNullOrEmpty(platformId))
            {
                _errAcc.AddError("Platform ID is null or empty");
            }

            if (platformData.PowerShell.Version == null)
            {
                _errAcc.AddError("PowerShell version is null");
            }
            else if (platformData.PowerShell.Version.Major >= 5
                    && string.IsNullOrEmpty(platformData.PowerShell.Edition))
            {
                _errAcc.AddError("PowerShell edition is null or empty on version 5.1 or above");
            }

            if (platformData.Dotnet.ClrVersion == null)
            {
                _errAcc.AddError(".NET CLR version is null");
            }

            if (string.IsNullOrEmpty(platformData.OperatingSystem.Name))
            {
                _errAcc.AddError("OS name is null or empty");
            }

            if (string.IsNullOrEmpty(platformData.OperatingSystem.Version))
            {
                _errAcc.AddError("OS version is null or empty");
            }
        }

        private void ValidateRuntimeData(RuntimeData runtimeData)
        {
            try
            {
                ValidateCommonData(runtimeData.Common);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }

            try
            {
                ValidateModules(runtimeData.Modules);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }

            try
            {
                ValidateAvailableTypes(runtimeData.Types);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }
        }

        private void ValidateCommonData(CommonPowerShellData commonData)
        {
            if (commonData == null)
            {
                _errAcc.AddError("CommonData is null");
                return;
            }

            if (commonData.Parameters == null)
            {
                _errAcc.AddError("CommonData.Parameters is null");
            }
            else
            {
                foreach (string commonParameter in s_commonParameters)
                {
                    if (!commonData.Parameters.TryGetValue(commonParameter, out ParameterData parameter))
                    {
                        _errAcc.AddError($"Common parameter {commonParameter} is not present");
                    }
                    else if (parameter == null)
                    {
                        _errAcc.AddError($"Common parameter {commonParameter} present but null");
                    }
                }
            }

            if (commonData.ParameterAliases == null)
            {
                _errAcc.AddError("CommonData.ParameterAliases is null");
            }
            else
            {
                foreach (string commonParameterAlias in s_commonParameterAliases)
                {
                    if (!commonData.ParameterAliases.TryGetValue(commonParameterAlias, out ParameterData parameter))
                    {
                        _errAcc.AddError($"Common parameter alias {commonParameterAlias} not present");
                    }
                    else if (parameter == null)
                    {
                        _errAcc.AddError($"Common parameter alias {commonParameterAlias} present but null");
                    }
                }
            }
        }

        private void ValidateModules(IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> modules)
        {
            if (modules == null)
            {
                _errAcc.AddError("RuntimeData.Modules is null");
                return;
            }

            foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedModule in s_modules)
            {
                ValidateSingleModule(expectedModule, modules);
            }

            foreach (KeyValuePair<string, IReadOnlyCollection<string>> expectedModuleAliases in s_aliases)
            {
                ValidateSingleModuleAliases(expectedModuleAliases, modules);
            }

            if (_psVersionMajor > 4)
            {
                foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedModule in s_ps4Modules)
                {
                    ValidateSingleModule(expectedModule, modules);
                }
            }

            if (_psVersionMajor > 5)
            {
                foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedModule in s_ps5Modules)
                {
                    ValidateSingleModule(expectedModule, modules);
                }

                foreach (KeyValuePair<string, IReadOnlyCollection<string>> expectedModuleAliases in s_ps5Aliases)
                {
                    ValidateSingleModuleAliases(expectedModuleAliases, modules);
                }
            }
        }

        private void ValidateSingleModule(
            KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedModule,
            IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> modules)
        {
            if (!modules.TryGetValue(expectedModule.Key, out IReadOnlyDictionary<Version, ModuleData> moduleVersions))
            {
                _errAcc.AddError($"Module {expectedModule.Key} is not present");
                return;
            }
            else if (moduleVersions.Count == 0)
            {
                _errAcc.AddError($"Module {expectedModule.Key} is present by name but has no data entries");
                return;
            }

            ModuleData module = moduleVersions.OrderByDescending(moduleVersion => moduleVersion.Key).First().Value;

            foreach (KeyValuePair<string, IReadOnlyCollection<string>> expectedCmdlet in expectedModule.Value)
            {
                if (!module.Cmdlets.TryGetValue(expectedCmdlet.Key, out CmdletData cmdlet))
                {
                    _errAcc.AddError($"Expected cmdlet {expectedCmdlet.Key} not found in module {expectedModule.Key}");
                    continue;
                }

                foreach (string expectedParameter in expectedCmdlet.Value)
                {
                    if (!cmdlet.Parameters.ContainsKey(expectedParameter))
                    {
                        _errAcc.AddError($"Expected parameter {expectedParameter} on cmdlet {expectedCmdlet.Key} in module {expectedModule.Key} was not found");
                    }
                }
            }
        }

        private void ValidateSingleModuleAliases(
            KeyValuePair<string, IReadOnlyCollection<string>> expectedAliases,
            IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> modules)
        {
            ModuleData module;
            try
            {
                module = modules[expectedAliases.Key].OrderByDescending(moduleVersion => moduleVersion.Key).First().Value;
            }
            catch (InvalidOperationException)
            {
                _errAcc.AddError($"Module {expectedAliases.Key} not found while checking aliases");
                return;
            }

            foreach (string expectedAlias in expectedAliases.Value)
            {
                if (!module.Aliases.ContainsKey(expectedAlias))
                {
                    _errAcc.AddError($"Expected alias {expectedAlias} was not found in module {expectedAliases.Key}");
                }
            }
        }

        private void ValidateAvailableTypes(AvailableTypeData types)
        {
            try
            {
                ValidateTypeAccelerators(types.TypeAccelerators);
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }

            try
            {
                ValidateAssemblies(types.Assemblies, s_types);

                switch (_dotNetEdition)
                {
                    case Data.DotnetRuntime.Core:
                        ValidateAssemblies(types.Assemblies, s_coreTypes);
                        break;

                    case Data.DotnetRuntime.Framework:
                        ValidateAssemblies(types.Assemblies, s_fxTypes);
                        break;

                    default:
                        _errAcc.AddError($"Dotnet edition did not resolve properly");
                        break;
                }
            }
            catch (Exception e)
            {
                _errAcc.AddError(e);
            }
        }

        private void ValidateTypeAccelerators(IReadOnlyDictionary<string, TypeAcceleratorData> typeAccelerators)
        {
            foreach (KeyValuePair<string, string> expectedTypeAccelerator in s_typeAccelerators)
            {
                if (!typeAccelerators.TryGetValue(expectedTypeAccelerator.Key, out TypeAcceleratorData typeAccelerator))
                {
                    _errAcc.AddError($"Expected type accelerator {expectedTypeAccelerator.Key} not found");
                    continue;
                }

                if (typeAccelerator == null)
                {
                    _errAcc.AddError($"Expected type accelerator {expectedTypeAccelerator.Key} found but null");
                    continue;
                }

                if (!typeAccelerator.Type.Equals(expectedTypeAccelerator.Value))
                {
                    _errAcc.AddError($"Type accelerator {expectedTypeAccelerator.Key} was expected to point to type {expectedTypeAccelerator.Value}, but instead points to {typeAccelerator.Type}");
                }
            }
        }

        private void ValidateAssemblies(
            IReadOnlyDictionary<string, AssemblyData> assemblies,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedAssemblies)
        {
            foreach (KeyValuePair<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> expectedAssembly in expectedAssemblies)
            {
                if (!assemblies.TryGetValue(expectedAssembly.Key, out AssemblyData assembly))
                {
                    _errAcc.AddError($"Expected assembly {expectedAssembly.Key} not found");
                    continue;
                }

                if (assembly == null)
                {
                    _errAcc.AddError($"Expected assembly {expectedAssembly.Key} was found but null");
                    continue;
                }

                foreach (KeyValuePair<string, IReadOnlyCollection<string>> expectedNamespace in expectedAssembly.Value)
                {
                    if (!assembly.Types.TryGetValue(expectedNamespace.Key, out IReadOnlyDictionary<string, TypeData> namespaceTypes))
                    {
                        _errAcc.AddError($"Assembly {expectedAssembly.Key} does not contain expected namespace {expectedNamespace.Key}");
                        continue;
                    }

                    if (namespaceTypes == null)
                    {
                        _errAcc.AddError($"Assembly {expectedAssembly.Key} contains namespace {expectedNamespace.Key} but it is null");
                        continue;
                    }

                    foreach (string expectedType in expectedNamespace.Value)
                    {
                        if (!namespaceTypes.TryGetValue(expectedType, out TypeData typeData))
                        {
                            _errAcc.AddError($"Expected type {expectedType} was not found in namespace {expectedNamespace.Key} in assembly {expectedAssembly.Key}");
                            continue;
                        }

                        if (typeData == null)
                        {
                            _errAcc.AddError($"Expected type {expectedType} was found but null in namespace {expectedNamespace.Key} in assemblye {expectedAssembly.Key}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Class to accumuate validator errors.
        /// </summary>
        private class ValidationErrorAccumulator
        {
            private List<Exception> _errors;

            public ValidationErrorAccumulator()
            {
                _errors = new List<Exception>();
            }

            public void AddError(string message)
            {
                _errors.Add(new Exception(message));
            }

            public void AddError(Exception exception)
            {
                _errors.Add(exception);
            }

            public IEnumerable<Exception> GetErrors()
            {
                return _errors;
            }

            public bool HasErrors()
            {
                return _errors.Count > 0;
            }

            public void Clear()
            {
                _errors.Clear();
            }
        }
    }
}