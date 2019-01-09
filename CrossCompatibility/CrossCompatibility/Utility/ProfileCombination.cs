using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Data.Modules;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Data.Types;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public static class ProfileCombination
    {
        public static CompatibilityProfileData IntersectMany(IEnumerable<CompatibilityProfileData> profiles)
        {
            return CombineProfiles(profiles, Intersect);
        }

        public static CompatibilityProfileData UnionMany(IEnumerable<CompatibilityProfileData> profiles)
        {
            return CombineProfiles(profiles, Union);
        }

        public static object Intersect(CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            thisProfile.Compatibility = (RuntimeData)Intersect(thisProfile.Compatibility, thatProfile.Compatibility);

            // Intersection of platforms is just adding them to the array
            var platforms = new PlatformData[thisProfile.Platforms.Length + thatProfile.Platforms.Length];
            thisProfile.Platforms.CopyTo(platforms, 0);
            thatProfile.Platforms.CopyTo(platforms, thisProfile.Platforms.Length);
            thisProfile.Platforms = platforms;

            return thisProfile;
        }

        public static object Intersect(RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            thisRuntime.Types = (AvailableTypeData)Intersect(thisRuntime.Types, thatRuntime.Types);

            // Intersect modules first at the whole module level
            thisRuntime.Modules = (JsonDictionary<string, JsonDictionary<Version, ModuleData>>)Intersect(thisRuntime.Modules, thatRuntime.Modules, keyComparer: StringComparer.OrdinalIgnoreCase);

            // TODO:
            //  - Don't intersect module versions as with other dictionary keys
            //  - Instead, union all modules in left and right respectively, then intersect
            foreach (KeyValuePair<string, JsonDictionary<Version, ModuleData>> moduleVersions in thatRuntime.Modules)
            {
                string name = moduleVersions.Key;

                if (!thisRuntime.Modules.ContainsKey(name))
                {
                    continue;
                }

                thisRuntime.Modules[name] = (JsonDictionary<Version, ModuleData>)Intersect(thisRuntime.Modules[name], thatRuntime.Modules[name], Intersect);
            }

            return thisRuntime;
        }

        public static object Intersect(AvailableTypeData thisTypes, AvailableTypeData thatTypes)
        {
            thisTypes.Assemblies = (JsonDictionary<string, AssemblyData>)Intersect(thisTypes.Assemblies, thatTypes.Assemblies, Intersect);

            thisTypes.TypeAccelerators = (JsonDictionary<string, TypeAcceleratorData>)Intersect(thisTypes.TypeAccelerators, thatTypes.TypeAccelerators);
            
            return thisTypes;
        }

        public static object Intersect(AssemblyData thisAsm, AssemblyData thatAsm)
        {
            if (thisAsm == thatAsm)
            {

            }

            thisAsm.AssemblyName = (AssemblyNameData)Intersect(thisAsm.AssemblyName, thatAsm.AssemblyName);

            thisAsm.Types = (JsonDictionary<string, JsonDictionary<string, TypeData>>)Intersect(thisAsm.Types, thatAsm.Types);
            
            if (thisAsm.Types != null)
            {
                if (thatAsm.Types == null)
                {
                    thisAsm.Types = null;
                }
                else
                {
                    foreach (KeyValuePair<string, JsonDictionary<string, TypeData>> typeNamespace in thatAsm.Types)
                    {
                        if (!thisAsm.Types.ContainsKey(typeNamespace.Key))
                        {
                            continue;
                        }

                        thisAsm.Types[typeNamespace.Key] = (JsonDictionary<string, TypeData>)Intersect(thisAsm.Types[typeNamespace.Key], typeNamespace.Value);
                    }
                }
            }

            return thisAsm;
        }

        public static object Intersect(AssemblyNameData thisAsmName, AssemblyNameData thatAsmName)
        {
            // Having different cultures downgrades to culture neutral
            if (thisAsmName.Culture != thatAsmName.Culture)
            {
                thisAsmName.Culture = null;
            }

            return thisAsmName;
        }

        public static object Intersect(TypeData thisType, TypeData thatType)
        {
            thisType.Instance = (MemberData)Intersect(thisType.Instance, thatType.Instance);
            thisType.Static = (MemberData)Intersect(thisType.Static, thatType.Static);

            return thisType;
        }

        public static object Intersect(MemberData thisMembers, MemberData thatMembers)
        {
            thisMembers.Events = (JsonDictionary<string, EventData>)Intersect(thisMembers.Events, thatMembers.Events);
            thisMembers.Fields = (JsonDictionary<string, FieldData>)Intersect(thisMembers.Fields, thatMembers.Fields);
            thisMembers.Properties = (JsonDictionary<string, PropertyData>)Intersect(thisMembers.Properties, thatMembers.Properties, Intersect);
            thisMembers.NestedTypes = (JsonDictionary<string, TypeData>)Intersect(thisMembers.NestedTypes, thatMembers.NestedTypes, Intersect);
            thisMembers.Methods = (JsonDictionary<string, MethodData>)Intersect(thisMembers.Methods, thatMembers.Methods);

            // Recollect only constructors that occur in both left and right sets
            var thisConstructors = new List<string[]>();
            foreach (string[] thisParams in thisMembers.Constructors)
            {
                foreach (string[] thatParams in thatMembers.Constructors)
                {
                    if (new ParameterListComparer().Equals(thisParams, thatParams))
                    {
                        thisConstructors.Add(thisParams);
                    }
                }
            }
            thisMembers.Constructors = thisConstructors.ToArray();

            if (thisMembers.Indexers != null)
            {
                if (thatMembers.Indexers == null)
                {
                    thisMembers.Indexers = null;
                }
                else
                {
                    // Recollect indexers that occur in both left and right sets
                    var thisIndexers = new List<IndexerData>();
                    foreach (IndexerData thisIndexer in thisMembers.Indexers)
                    {
                        foreach (IndexerData thatIndexer in thatMembers.Indexers)
                        {
                            if (new ParameterListComparer().Equals(thisIndexer.Parameters, thatIndexer.Parameters))
                            {
                                IndexerData indexer = (IndexerData)Intersect(thisIndexer, thatIndexer);
                                thisIndexers.Add(indexer);
                            }
                        }
                    }
                    thisMembers.Indexers = thisIndexers.ToArray();
                }
            }

            return thisMembers;
        }

        public static object Intersect(IndexerData thisIndexer, IndexerData thatIndexer)
        {
            thisIndexer.Accessors = thisIndexer.Accessors.Intersect(thatIndexer.Accessors).ToArray();
            return thisIndexer;
        }

        public static object Intersect(PropertyData thisProperty, PropertyData thatProperty)
        {
            thisProperty.Accessors = thisProperty.Accessors.Intersect(thatProperty.Accessors).ToArray();
            return thisProperty;
        }

        public static object Intersect(ModuleData thisModule, ModuleData thatModule)
        {
            thisModule.Aliases = (JsonDictionary<string, string>)Intersect(thisModule.Aliases, thatModule.Aliases);
            thisModule.Variables = thisModule.Variables?.Intersect(thatModule.Variables ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase).ToArray();
            thisModule.Cmdlets = (JsonDictionary<string, CmdletData>)Intersect(thisModule.Cmdlets, thatModule.Cmdlets, Intersect);
            thisModule.Functions = (JsonDictionary<string, FunctionData>)Intersect(thisModule.Functions, thatModule.Functions, Intersect);

            return thisModule;
        }

        public static object Intersect(CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = thisCommand.OutputType?.Intersect(thatCommand.OutputType ?? Enumerable.Empty<string>()).ToArray();
            thisCommand.ParameterSets = thisCommand.ParameterSets?.Intersect(thatCommand.ParameterSets ?? Enumerable.Empty<string>()).ToArray();

            thisCommand.ParameterAliases = (JsonDictionary<string, string>)Intersect(thisCommand.ParameterAliases, thatCommand.ParameterAliases);
            thisCommand.Parameters = (JsonDictionary<string, ParameterData>)Intersect(thisCommand.Parameters, thatCommand.Parameters, Intersect);

            return thisCommand;
        }

        public static object Intersect(ParameterData thisParam, ParameterData thatParam)
        {
            thisParam.ParameterSets = (JsonDictionary<string, ParameterSetData>)Intersect(thisParam.ParameterSets, thatParam.ParameterSets);
            return thisParam;
        }

        public static object Union(CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            // There's no simple solution to this currently.
            // We can revisit, but platform unions don't make much sense out of context
            thisProfile.Platforms = null;

            Union(thisProfile.Compatibility, thatProfile.Compatibility);

            return thisProfile;
        }

        public static object Union(RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            foreach (KeyValuePair<string, JsonDictionary<Version, ModuleData>> moduleVersions in thatRuntime.Modules)
            {
                if (!thisRuntime.Modules.ContainsKey(moduleVersions.Key))
                {
                    thisRuntime.Modules.Add(moduleVersions);
                    continue;
                }

                thisRuntime.Modules[moduleVersions.Key] = DictionaryUnion(thisRuntime.Modules[moduleVersions.Key], moduleVersions.Value);
            }

            Union(thisRuntime.Types, thatRuntime.Types);

            return thisRuntime;
        }

        public static object Union(ModuleData thisModule, ModuleData thatModule)
        {
            thisModule.Aliases = DictionaryUnion(thisModule.Aliases, thatModule.Aliases);

            thisModule.Variables = ArrayUnion(thisModule.Variables, thatModule.Variables);

            thisModule.Cmdlets = DictionaryUnion(thisModule.Cmdlets, thatModule.Cmdlets, Union);

            thisModule.Functions = DictionaryUnion(thisModule.Functions, thatModule.Functions, Union);

            return thisModule;
        }

        public static object Union(CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = ArrayUnion(thisCommand.OutputType, thatCommand.OutputType);
            thisCommand.ParameterSets = ArrayUnion(thisCommand.ParameterSets, thatCommand.ParameterSets);

            thisCommand.ParameterAliases = DictionaryUnion(thisCommand.ParameterAliases, thatCommand.ParameterAliases);
            thisCommand.Parameters = DictionaryUnion(thisCommand.Parameters, thatCommand.Parameters, Union);

            return thisCommand;
        }

        public static object Union(ParameterData thisParameter, ParameterData thatParameter)
        {
            thisParameter.ParameterSets = DictionaryUnion(thisParameter.ParameterSets, thatParameter.ParameterSets, Union);

            return thisParameter;
        }

        public static object Union(ParameterSetData thisParameterSet, ParameterSetData thatParameterSet)
        {
            thisParameterSet.Flags = ArrayUnion(thisParameterSet.Flags, thatParameterSet.Flags);

            return thisParameterSet;
        }

        public static object Union(AvailableTypeData thisTypes, AvailableTypeData thatTypes)
        {
            thisTypes.Assemblies = DictionaryUnion(thisTypes.Assemblies, thatTypes.Assemblies, Union);
            thisTypes.TypeAccelerators = DictionaryUnion(thisTypes.TypeAccelerators, thatTypes.TypeAccelerators);

            return thisTypes;
        }

        public static object Union(AssemblyData thisAssembly, AssemblyData thatAssembly)
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
                        thisAssembly.Types.Add(nspace);
                        continue;
                    }

                    thisAssembly.Types[nspace.Key] = DictionaryUnion(thisAssembly.Types[nspace.Key], nspace.Value, Union);
                }
            }

            return thisAssembly;
        }

        public static object Union(AssemblyNameData thisAsmName, AssemblyNameData thatAsmName)
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

        public static object Union(TypeData thisType, TypeData thatType)
        {
            thisType.Instance = (MemberData)Union(thisType.Instance, thatType.Instance);
            thisType.Static = (MemberData)Union(thisType.Instance, thatType.Instance);
            return thisType;
        }

        public static object Union(MemberData thisMembers, MemberData thatMembers)
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

        public static object Union(PropertyData thisProperty, PropertyData thatProperty)
        {
            thisProperty.Accessors = ArrayUnion(thisProperty.Accessors, thatProperty.Accessors);
            return thisProperty;
        }

        public static object Union(MethodData thisMethod, MethodData thatMethod)
        {
            thisMethod.OverloadParameters = ParameterUnion(thisMethod.OverloadParameters, thatMethod.OverloadParameters);
            return thisMethod;
        }

        private static object Intersect<K, V>(
            JsonDictionary<K, V> thisDict,
            JsonDictionary<K, V> thatDict,
            Func<V, V, object> intersector = null,
            IEqualityComparer<K> keyComparer = null)
            where K : ICloneable
            where V : ICloneable
        {
            if (thatDict == null)
            {
                return thisDict;
            }

            if (thisDict == null)
            {
                return thatDict.Clone();
            }

            // Remove all the keys from left that aren't in right (and rest easy that we never added keys from right into left)
            foreach (K thisKey in thisDict.Keys.ToArray())
            {
                if (!thatDict.ContainsKey(thisKey))
                {
                    thisDict.Remove(thisKey);
                    continue;
                }

                if (intersector != null)
                {
                    thisDict[thisKey] = (V)intersector(thisDict[thisKey], thatDict[thisKey]);
                }
            }

            return thisDict;
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

            if (thisArray == null)
            {
                return (T[])thatArray.Clone();
            }

            return thisArray.Union(thatArray).ToArray();
        }

        private static string[][] ParameterUnion(string[][] thisParameters, string[][] thatParameters)
        {
            var parameters = new HashSet<string[]>(thisParameters, new ParameterListComparer());

            foreach (string[] thatParameter in thatParameters)
            {
                parameters.Add(thatParameter);
            }

            return parameters.ToArray();
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
                    thisDict.Add(item);
                    continue;
                }

                if (valueUnionizer != null)
                {
                    thisDict[item.Key] = (V)valueUnionizer(thisDict[item.Key], item.Value);
                }
            }

            return thisDict;
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
                unsafe
                {
                    int hc = 1;
                    foreach (string s in obj)
                    {
                        hc = 31 * hc + s.GetHashCode();
                    }
                    return hc;
                }
            }
        }
    }
}