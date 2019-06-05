// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// API to combine PowerShell compatibility profiles together.
    /// Currently this only supports unions, used to generate a union profile
    /// so that commands and types can be identified as being in some profile but not a configured one.
    /// </summary>
    public static class ProfileCombination
    {
        /// <summary>
        /// Combine a list of compatibility profiles together so that any assembly, module,
        /// type, command, etc. available in one is listed in the final result.
        /// </summary>
        /// <param name="profileId">The profile ID to assign to the generated union profile.</param>
        /// <param name="profiles">The profiles to union together to generate the result.</param>
        /// <returns>The deep union of all the given profiles, with the configured ID.</returns>
        public static CompatibilityProfileData UnionMany(string profileId, IEnumerable<CompatibilityProfileData> profiles)
        {
            CompatibilityProfileData unionProfile = CombineProfiles(profiles, Union);

            unionProfile.Platform = null;
            unionProfile.Id = profileId;
            unionProfile.ConstituentProfiles = profiles.Select(p => p.Id).ToArray();
            return unionProfile;
        }

        private static object Union(CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            Union(thisProfile.Runtime, thatProfile.Runtime);

            return thisProfile;
        }

        private static object Union(RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            foreach (KeyValuePair<string, JsonDictionary<Version, ModuleData>> moduleVersions in thatRuntime.Modules)
            {
                if (!thisRuntime.Modules.ContainsKey(moduleVersions.Key))
                {
                    thisRuntime.Modules.Add(moduleVersions.Key, moduleVersions.Value);
                    continue;
                }

                thisRuntime.Modules[moduleVersions.Key] = DictionaryUnion(thisRuntime.Modules[moduleVersions.Key], moduleVersions.Value, Union);
            }

            thisRuntime.NativeCommands = StringDictionaryUnion(thisRuntime.NativeCommands, thatRuntime.NativeCommands, ArrayUnion);

            thisRuntime.Common = (CommonPowerShellData)Union(thisRuntime.Common, thatRuntime.Common);

            Union(thisRuntime.Types, thatRuntime.Types);

            return thisRuntime;
        }

        private static object Union(CommonPowerShellData thisCommon, CommonPowerShellData thatCommon)
        {
            if (thatCommon == null)
            {
                return thisCommon;
            }

            if (thisCommon == null)
            {
                return thatCommon.Clone();
            }

            thisCommon.ParameterAliases = StringDictionaryUnion(thisCommon.ParameterAliases, thatCommon.ParameterAliases);
            thisCommon.Parameters = StringDictionaryUnion(thisCommon.Parameters, thatCommon.Parameters, Union);
            return thisCommon;
        }

        private static object Union(ModuleData thisModule, ModuleData thatModule)
        {
            thisModule.Aliases = StringDictionaryUnion(thisModule.Aliases, thatModule.Aliases);

            thisModule.Variables = ArrayUnion(thisModule.Variables, thatModule.Variables);

            thisModule.Cmdlets = StringDictionaryUnion(thisModule.Cmdlets, thatModule.Cmdlets, Union);

            thisModule.Functions = StringDictionaryUnion(thisModule.Functions, thatModule.Functions, Union);

            return thisModule;
        }

        private static object Union(CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = ArrayUnion(thisCommand.OutputType, thatCommand.OutputType);
            thisCommand.ParameterSets = ArrayUnion(thisCommand.ParameterSets, thatCommand.ParameterSets);

            thisCommand.ParameterAliases = StringDictionaryUnion(thisCommand.ParameterAliases, thatCommand.ParameterAliases);
            thisCommand.Parameters = StringDictionaryUnion(thisCommand.Parameters, thatCommand.Parameters, Union);

            return thisCommand;
        }

        private static object Union(ParameterData thisParameter, ParameterData thatParameter)
        {
            thisParameter.ParameterSets = StringDictionaryUnion(thisParameter.ParameterSets, thatParameter.ParameterSets, Union);

            return thisParameter;
        }

        private static object Union(ParameterSetData thisParameterSet, ParameterSetData thatParameterSet)
        {
            thisParameterSet.Flags = ArrayUnion(thisParameterSet.Flags, thatParameterSet.Flags);

            return thisParameterSet;
        }

        private static object Union(AvailableTypeData thisTypes, AvailableTypeData thatTypes)
        {
            thisTypes.Assemblies = DictionaryUnion(thisTypes.Assemblies, thatTypes.Assemblies, Union);
            thisTypes.TypeAccelerators = StringDictionaryUnion(thisTypes.TypeAccelerators, thatTypes.TypeAccelerators);

            return thisTypes;
        }

        private static object Union(AssemblyData thisAssembly, AssemblyData thatAssembly)
        {
            Union(thisAssembly.AssemblyName, thatAssembly.AssemblyName);

            if (thatAssembly.Types != null)
            {
                if (thisAssembly.Types == null)
                {
                    thisAssembly.Types = new JsonDictionary<string, JsonDictionary<string, TypeData>>();
                }

                foreach (KeyValuePair<string, JsonDictionary<string, TypeData>> nspace in thatAssembly.Types)
                {
                    if (!thisAssembly.Types.ContainsKey(nspace.Key))
                    {
                        thisAssembly.Types.Add(nspace.Key, nspace.Value);
                        continue;
                    }

                    thisAssembly.Types[nspace.Key] = DictionaryUnion(thisAssembly.Types[nspace.Key], nspace.Value, Union);
                }
            }

            return thisAssembly;
        }

        private static object Union(AssemblyNameData thisAsmName, AssemblyNameData thatAsmName)
        {
            if (thatAsmName.Version > thisAsmName.Version)
            {
                thisAsmName.Version = thatAsmName.Version;
            }

            if (thisAsmName.PublicKeyToken == null && thatAsmName.PublicKeyToken != null)
            {
                thisAsmName.PublicKeyToken = thatAsmName.PublicKeyToken;
            }

            return thisAsmName;
        }

        private static object Union(TypeData thisType, TypeData thatType)
        {
            thisType.Instance = (MemberData)Union(thisType.Instance, thatType.Instance);
            thisType.Static = (MemberData)Union(thisType.Static, thatType.Static);
            return thisType;
        }

        private static object Union(MemberData thisMembers, MemberData thatMembers)
        {
            if (thatMembers == null)
            {
                return thisMembers;
            }

            if (thisMembers == null)
            {
                return thatMembers.Clone();
            }

            thisMembers.Indexers = ArrayUnion(thisMembers.Indexers, thatMembers.Indexers);

            thisMembers.Constructors = ParameterUnion(thisMembers.Constructors, thatMembers.Constructors);

            thisMembers.Events = DictionaryUnion(thisMembers.Events, thatMembers.Events);
            thisMembers.Fields = DictionaryUnion(thisMembers.Fields, thatMembers.Fields);
            thisMembers.Methods = DictionaryUnion(thisMembers.Methods, thatMembers.Methods, Union);
            thisMembers.NestedTypes = DictionaryUnion(thisMembers.NestedTypes, thatMembers.NestedTypes, Union);
            thisMembers.Properties = DictionaryUnion(thisMembers.Properties, thatMembers.Properties, Union);

            return thisMembers;
        }

        private static object Union(PropertyData thisProperty, PropertyData thatProperty)
        {
            thisProperty.Accessors = ArrayUnion(thisProperty.Accessors, thatProperty.Accessors);
            return thisProperty;
        }

        private static object Union(MethodData thisMethod, MethodData thatMethod)
        {
            thisMethod.OverloadParameters = ParameterUnion(thisMethod.OverloadParameters, thatMethod.OverloadParameters);
            return thisMethod;
        }

        private static CompatibilityProfileData CombineProfiles(IEnumerable<CompatibilityProfileData> profiles, Func<CompatibilityProfileData, CompatibilityProfileData, object> combinator)
        {
            IEnumerator<CompatibilityProfileData> profileEnumerator = profiles.GetEnumerator();

            if (!profileEnumerator.MoveNext())
            {
                return null;
            }

            CompatibilityProfileData mutProfileBase = (CompatibilityProfileData)profileEnumerator.Current.Clone();

            while(profileEnumerator.MoveNext())
            {
                mutProfileBase = (CompatibilityProfileData)combinator(mutProfileBase, profileEnumerator.Current);
            }

            return mutProfileBase;
        }

        private static T[] ArrayUnion<T>(T[] thisArray, T[] thatArray)
        {
            if (thatArray == null)
            {
                return thisArray;
            }

            bool canClone = typeof(ICloneable).IsAssignableFrom(typeof(T));

            var clonedThat = new T[thatArray.Length];
            if (canClone)
            {
                for (int i = 0; i < thatArray.Length; i++)
                {
                    clonedThat[i] = (T)((dynamic)thatArray[i]).Clone();
                }
            }
            else
            {
                for (int i = 0; i < thatArray.Length; i++)
                {
                    clonedThat[i] = (T)thatArray[i];
                }
            }

            if (thisArray == null)
            {
                return clonedThat;
            }

            return thisArray.Union(thatArray).ToArray();
        }

        private static string[][] ParameterUnion(string[][] thisParameters, string[][] thatParameters)
        {
            if (thatParameters == null)
            {
                return thisParameters;
            }

            if (thisParameters == null)
            {
                return thatParameters.Select(arr => (string[])arr.Clone()).ToArray();
            }

            var parameters = new HashSet<string[]>(thisParameters, new ParameterListComparer());

            foreach (string[] thatParameter in thatParameters)
            {
                parameters.Add(thatParameter);
            }

            return parameters.ToArray();
        }

        private static JsonCaseInsensitiveStringDictionary<TValue> StringDictionaryUnion<TValue>(
            JsonCaseInsensitiveStringDictionary<TValue> thisStringDict,
            JsonCaseInsensitiveStringDictionary<TValue> thatStringDict,
            Func<TValue, TValue, object> valueUnionizer = null)
            where TValue : ICloneable
        {
            if (thatStringDict == null)
            {
                return thisStringDict;
            }

            if (thisStringDict == null)
            {
                return (JsonCaseInsensitiveStringDictionary<TValue>)thatStringDict.Clone();
            }

            foreach (KeyValuePair<string, TValue> item in thatStringDict)
            {
                if (!thisStringDict.ContainsKey(item.Key))
                {
                    thisStringDict.Add(item.Key, item.Value);
                    continue;
                }

                if (valueUnionizer != null)
                {
                    thisStringDict[item.Key] = (TValue)valueUnionizer(thisStringDict[item.Key], item.Value);
                }
            }

            return thisStringDict;
        }

        private static JsonDictionary<K, V> DictionaryUnion<K, V>(
            JsonDictionary<K, V> thisDict,
            JsonDictionary<K, V> thatDict, 
            Func<V, V, object> valueUnionizer = null)
            where K : ICloneable where V : ICloneable
        {
            if (thatDict == null)
            {
                return thisDict;
            }

            if (thisDict == null)
            {
                return (JsonDictionary<K, V>)thatDict.Clone();
            }

            foreach (KeyValuePair<K, V> item in thatDict)
            {
                if (!thisDict.ContainsKey(item.Key))
                {
                    thisDict.Add(item.Key, item.Value);
                    continue;
                }

                if (valueUnionizer != null)
                {
                    thisDict[item.Key] = (V)valueUnionizer(thisDict[item.Key], item.Value);
                }
            }

            return thisDict;
        }

        private static KeyValuePair<Version, ModuleData> UnionVersionedModules(IReadOnlyCollection<KeyValuePair<Version, ModuleData>> modules)
        {
            ModuleData unionedModule = null;
            Version version = null;
            bool firstModule = true;
            foreach (KeyValuePair<Version, ModuleData> modVersion in modules)
            {
                if (firstModule)
                {
                    version = modVersion.Key;
                    unionedModule = (ModuleData)modVersion.Value.Clone();
                    firstModule = false;
                    continue;
                }

                version = version >= modVersion.Key ? version : modVersion.Key;
                unionedModule = (ModuleData)Union(unionedModule, modVersion.Value);
            }

            return new KeyValuePair<Version, ModuleData>(version, unionedModule);
        }


        private struct ParameterListComparer : IEqualityComparer<string[]>
        {
            public bool Equals(string[] x, string[] y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(string[] obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                int hc = 1;
                foreach (string s in obj)
                {
                    unchecked
                    {
                        hc = 31 * hc + (s?.GetHashCode() ?? 0);
                    }
                }
                return hc;
            }
        }
    }
}
