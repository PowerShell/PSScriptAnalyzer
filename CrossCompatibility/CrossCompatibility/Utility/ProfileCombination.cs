using System;
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
        public static void Intersect(CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            Intersect(thisProfile.Compatibility, thatProfile.Compatibility);

            // Intersection of platforms is just adding them to the array
            var platforms = new PlatformData[thisProfile.Platforms.Length + thatProfile.Platforms.Length];
            thisProfile.Platforms.CopyTo(platforms, 0);
            thatProfile.Platforms.CopyTo(platforms, thisProfile.Platforms.Length);
            thisProfile.Platforms = platforms;
        }

        public static void Intersect(RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            Intersect(thisRuntime.Types, thatRuntime.Types);

            // Intersect modules first at the whole module level
            TryIntersect(thisRuntime.Modules, thatRuntime.Modules, StringComparer.OrdinalIgnoreCase);

            // Then strip out parts that are not common
            foreach (KeyValuePair<string, ModuleData> module in thisRuntime.Modules)
            {
                Intersect(module.Value, thatRuntime.Modules[module.Key]);
            }
        }

        public static void Intersect(AvailableTypeData thisTypes, AvailableTypeData thatTypes)
        {
            TryIntersect(thisTypes.Assemblies, thatTypes.Assemblies);

            foreach (KeyValuePair<string, AssemblyData> assembly in thisTypes.Assemblies)
            {
                Intersect(assembly.Value, thatTypes.Assemblies[assembly.Key]);
            }

            TryIntersect(thisTypes.TypeAccelerators, thatTypes.TypeAccelerators);
        }

        public static void Intersect(AssemblyData thisAsm, AssemblyData thatAsm)
        {
            Intersect(thisAsm.AssemblyName, thatAsm.AssemblyName);

            thisAsm.Types.Intersect(thatAsm.Types);
            foreach (KeyValuePair<string, IDictionary<string, TypeData>> typeNamespace in thisAsm.Types)
            {
                typeNamespace.Value.Intersect(thatAsm.Types[typeNamespace.Key]);

                foreach (KeyValuePair<string, TypeData> type in typeNamespace.Value)
                {
                    Intersect(type.Value, thatAsm.Types[typeNamespace.Key][type.Key]);
                }
            }
        }

        public static void Intersect(AssemblyNameData thisAsmName, AssemblyNameData thatAsmName)
        {
            // Having different cultures downgrades to culture neutral
            if (thisAsmName.Culture != thatAsmName.Culture)
            {
                thisAsmName.Culture = null;
            }
        }

        public static void Intersect(TypeData thisType, TypeData thatType)
        {
            Intersect(thisType.Instance, thatType.Instance);
            Intersect(thisType.Static, thatType.Static);
        }

        public static void Intersect(MemberData thisMembers, MemberData thatMembers)
        {
            if (!TryIntersect(thisMembers.Events, thatMembers.Events))
            {
                thisMembers.Events = null;
            }

            if (!TryIntersect(thisMembers.Fields, thatMembers.Fields))
            {
                thisMembers.Fields = null;
            }

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

            // Recollect indexers that occur in both left and right sets
            var thisIndexers = new List<IndexerData>();
            foreach (IndexerData thisIndexer in thisMembers.Indexers)
            {
                foreach (IndexerData thatIndexer in thatMembers.Indexers)
                {
                    if (new ParameterListComparer().Equals(thisIndexer.Parameters, thatIndexer.Parameters))
                    {
                        Intersect(thisIndexer, thatIndexer);
                        thisIndexers.Add(thisIndexer);
                    }
                }
            }
            thisMembers.Indexers = thisIndexers.ToArray();

            if (thisMembers.Properties != null)
            {
                thisMembers.Properties?.Intersect(thatMembers.Properties);
                foreach (KeyValuePair<string, PropertyData> property in thisMembers.Properties)
                {
                    Intersect(property.Value, thatMembers.Properties[property.Key]);
                }
            }

            thisMembers.NestedTypes?.Intersect(thatMembers.NestedTypes);
            foreach (KeyValuePair<string, TypeData> type in thisMembers.NestedTypes)
            {
                Intersect(type.Value, thatMembers.NestedTypes[type.Key]);
            }
        }

        public static void Intersect(IndexerData thisIndexer, IndexerData thatIndexer)
        {
            thisIndexer.Accessors = thisIndexer.Accessors.Intersect(thatIndexer.Accessors).ToArray();
        }

        public static void Intersect(PropertyData thisProperty, PropertyData thatProperty)
        {
            thisProperty.Accessors = thisProperty.Accessors.Intersect(thatProperty.Accessors).ToArray();
        }

        public static void Intersect(ModuleData thisModule, ModuleData thatModule)
        {
            // Take the lower version of the module -- this is a hacky assumption, but better than the alternative
            thisModule.Version = thisModule.Version <= thatModule.Version ? thisModule.Version : thatModule.Version;

            thisModule.Aliases?.Intersect(thatModule.Aliases);

            thisModule.Variables?.Intersect(thatModule.Variables ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase).ToArray();

            if (thisModule.Cmdlets != null)
            {
                thisModule.Cmdlets.Intersect(thatModule.Cmdlets);

                foreach (KeyValuePair<string, CmdletData> thisCmdlet in thisModule.Cmdlets)
                {
                    Intersect(thisCmdlet.Value, thatModule.Cmdlets[thisCmdlet.Key]);
                }
            }

            if (thisModule.Functions != null)
            {
                thisModule.Functions.Intersect(thatModule.Functions);

                foreach (KeyValuePair<string, FunctionData> thisFunction in thisModule.Functions)
                {
                    if (thisModule.Functions[thisFunction.Key].CmdletBinding)
                    {
                        thisFunction.Value.CmdletBinding = true;
                    }
                    Intersect(thisFunction.Value, thatModule.Functions[thisFunction.Key]);
                }
            }
        }

        public static void Intersect(CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = thisCommand.OutputType?.Intersect(thatCommand.OutputType ?? Enumerable.Empty<string>()).ToArray();
            thisCommand.ParameterAliases?.Intersect(thatCommand?.ParameterAliases);
            thisCommand.ParameterSets = thisCommand.ParameterSets?.Intersect(thatCommand.ParameterSets ?? Enumerable.Empty<string>()).ToArray();

            if (thisCommand.Parameters != null)
            {
                thisCommand.Parameters?.Intersect(thatCommand.Parameters);

                foreach (KeyValuePair<string, ParameterData> parameter in thisCommand.Parameters)
                {
                    Intersect(parameter.Value, thatCommand.Parameters[parameter.Key]);
                }
            }
        }

        public static void Intersect(ParameterData thisParam, ParameterData thatParam)
        {
            thisParam.ParameterSets?.Intersect(thatParam.ParameterSets);
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
            thisRuntime.Modules = DictionaryUnion(thisRuntime.Modules, thatRuntime.Modules, Union);

            Union(thisRuntime.Types, thatRuntime.Types);

            return thisRuntime;
        }

        public static object Union(ModuleData thisModule, ModuleData thatModule)
        {
            if (thatModule.Version > thisModule.Version)
            {
                thisModule.Version = thatModule.Version;
            }

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

            foreach (KeyValuePair<string, IDictionary<string, TypeData>> nspace in thatAssembly.Types)
            {
                if (!thisAssembly.Types.ContainsKey(nspace.Key))
                {
                    thisAssembly.Types.Add(nspace);
                    continue;
                }

                thisAssembly.Types[nspace.Key] = DictionaryUnion(thisAssembly.Types[nspace.Key], nspace.Value, Union);
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

        private static bool TryIntersect<K, V>(IDictionary<K, V> thisDict, IDictionary<K, V> thatDict, IEqualityComparer<K> keyComparer = null)
        {
            if (thisDict == null || thatDict == null)
            {
                return false;
            }

            // Remove all keys in right outer join from this
            foreach (K thatKey in thatDict.Keys)
            {
                if (!thisDict.ContainsKey(thatKey))
                {
                    thisDict.Remove(thatKey);
                }
            }

            // Remove all keys in left outer join from this, being careful not to enumerate while mutating

            // Add keys to removal set
            var keysToRemove = keyComparer == null ? new HashSet<K>() : new HashSet<K>(keyComparer);
            foreach (K thisKey in thisDict.Keys)
            {
                if (!thatDict.ContainsKey(thisKey))
                {
                    keysToRemove.Add(thisKey);
                }
            }

            // Remove keys
            foreach (K keyToRemove in keysToRemove)
            {
                thisDict.Remove(keyToRemove);
            }

            return true;
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

        private static IDictionary<K, V> DictionaryUnion<K, V>(
            IDictionary<K, V> thisDict,
            IDictionary<K, V> thatDict, 
            Func<V, V, object> valueUnionizer = null)
            where K : ICloneable where V : ICloneable
        {
            if (thatDict == null)
            {
                return thisDict;
            }

            if (thisDict == null)
            {
                return thatDict.Clone();
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

        private static IDictionary<K, V> Clone<K, V>(this IDictionary<K, V> dict, IEqualityComparer<K> keyComparer = null)
            where K : ICloneable where V : ICloneable
        {
            var newDict = keyComparer == null
                ? new Dictionary<K, V>(dict.Count)
                : new Dictionary<K, V>(dict.Count, keyComparer);

            foreach (K key in dict.Keys)
            {
                newDict.Add((K)key.Clone(), (V)dict[key].Clone());
            }

            return newDict;
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