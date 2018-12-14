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
                    if (AreParametersMatching(thisParams, thatParams))
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
                    if (AreParametersMatching(thisIndexer.Parameters, thatIndexer.Parameters))
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

        public static void Union(CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            // There's no simple solution to this currently.
            // We can revisit, but platform unions don't make much sense out of context
            thisProfile.Platforms = null;

            Union(thisProfile.Compatibility, thatProfile.Compatibility);
        }

        public static void Union(RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            foreach (KeyValuePair<string, ModuleData> thatModule in thatRuntime.Modules)
            {
                // Add whole new modules from RHS
                if (!thisRuntime.Modules.ContainsKey(thatModule.Key))
                {
                    thisRuntime.Modules.Add(thatModule);
                    continue;
                }

                // Merge common modules
                Union(thisRuntime.Modules[thatModule.Key], thatModule.Value);
            }

            Union(thisRuntime.Types, thatRuntime.Types);
        }

        public static void Union(ModuleData thisModule, ModuleData thatModule)
        {
            if (thatModule.Version > thisModule.Version)
            {
                thisModule.Version = thatModule.Version;
            }

            if (TryNaiveUnion(thisModule.Aliases, thatModule.Aliases, out IDictionary<string, string> aliasDict))
            {
                thisModule.Aliases = aliasDict;
            }
            else
            {
                foreach (KeyValuePair<string, string> alias in thatModule.Aliases)
                {
                    thisModule.Aliases[alias.Key] = alias.Value;
                }
            }

            thisModule.Variables = ArrayUnion(thisModule.Variables, thatModule.Variables);

            if (TryNaiveUnion(thisModule.Cmdlets, thatModule.Cmdlets, out IDictionary<string, CmdletData> cmdletDictionary))
            {
                thisModule.Cmdlets = cmdletDictionary;
            }
            else
            {
                foreach (KeyValuePair<string, CmdletData> cmdlet in thatModule.Cmdlets)
                {
                    if (!thisModule.Cmdlets.ContainsKey(cmdlet.Key))
                    {
                        thisModule.Cmdlets.Add(cmdlet);
                        continue;
                    }

                    Union(thisModule.Cmdlets[cmdlet.Key], cmdlet.Value);
                }
            }

            if (TryNaiveUnion(thisModule.Functions, thatModule.Functions, out IDictionary<string, FunctionData> functionDictionary))
            {
                thisModule.Functions = functionDictionary;
            }
            else
            {
                foreach (KeyValuePair<string, FunctionData> function in thatModule.Functions)
                {
                    if (!thisModule.Functions.ContainsKey(function.Key))
                    {
                        thisModule.Functions.Add(function);
                        continue;
                    }

                    Union(thisModule.Cmdlets[function.Key], function.Value);
                }
            }

            if (thatModule.Functions != null)
            {
                if (thisModule.Functions == null)
                {
                    thisModule.Functions = thatModule.Functions.Clone();
                }
                else
                {
                    foreach (KeyValuePair<string, FunctionData> function in thatModule.Functions)
                    {
                        if (!thisModule.Functions.ContainsKey(function.Key))
                        {
                            thisModule.Functions.Add(function);
                            continue;
                        }

                        Union(thisModule.Functions[function.Key], function.Value);
                    }
                }
            }
        }

        public static void Union(CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = ArrayUnion(thisCommand.OutputType, thatCommand.OutputType);
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

        private static bool TryNaiveUnion<K, V>(
            IDictionary<K, V> thisDict,
            IDictionary<K, V> thatDict, 
            out IDictionary<K, V> result)
            where K : ICloneable where V : ICloneable
        {
            if (thatDict == null)
            {
                result = thisDict;
                return true;
            }

            if (thisDict == null)
            {
                result = thatDict.Clone();
                return true;
            }

            result = null;
            return false;
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

        private static bool AreParametersMatching(IReadOnlyList<string> thisParams, IReadOnlyList<string> thatParams)
        {
            if (thisParams.Count != thatParams.Count)
            {
                return false;
            }

            for (int i = 0; i < thisParams.Count; i++)
            {
                if (thisParams[i] != thatParams[i])
                {
                    return false;
                }
            }

            return true;
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
    }
}