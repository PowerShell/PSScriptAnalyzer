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
        public static void Intersect(this CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            thisProfile.Compatibility.Intersect(thatProfile.Compatibility);

            // Intersection of platforms is just adding them to the array
            var platforms = new PlatformData[thisProfile.Platforms.Length + thatProfile.Platforms.Length];
            thisProfile.Platforms.CopyTo(platforms, 0);
            thatProfile.Platforms.CopyTo(platforms, thisProfile.Platforms.Length);
            thisProfile.Platforms = platforms;
        }

        public static void Intersect(this RuntimeData thisRuntime, RuntimeData thatRuntime)
        {
            thisRuntime.Types.Intersect(thatRuntime.Types);

            // Intersect modules first at the whole module level
            thisRuntime.Modules.Intersect(thatRuntime.Modules, StringComparer.OrdinalIgnoreCase);

            // Then strip out parts that are not common
            foreach (KeyValuePair<string, ModuleData> module in thisRuntime.Modules)
            {
                module.Value.Intersect(thatRuntime.Modules[module.Key]);
            }
        }

        public static void Intersect(this AvailableTypeData thisTypes, AvailableTypeData thatTypes)
        {
            thisTypes.Assemblies.Intersect(thatTypes.Assemblies);

            foreach (KeyValuePair<string, AssemblyData> assembly in thisTypes.Assemblies)
            {
                assembly.Value.Intersect(thatTypes.Assemblies[assembly.Key]);
            }

            thisTypes.TypeAccelerators?.Intersect(thatTypes.TypeAccelerators);
        }

        public static void Intersect(this AssemblyData thisAsm, AssemblyData thatAsm)
        {
            thisAsm.AssemblyName.Intersect(thatAsm.AssemblyName);

            thisAsm.Types.Intersect(thatAsm.Types);
            foreach (KeyValuePair<string, IDictionary<string, TypeData>> typeNamespace in thisAsm.Types)
            {
                typeNamespace.Value.Intersect(thatAsm.Types[typeNamespace.Key]);

                foreach (KeyValuePair<string, TypeData> type in typeNamespace.Value)
                {
                    type.Value.Intersect(thatAsm.Types[typeNamespace.Key][type.Key]);
                }
            }
        }

        public static void Intersect(this AssemblyNameData thisAsmName, AssemblyNameData thatAsmName)
        {
            // Having different cultures downgrades to culture neutral
            if (thisAsmName.Culture != thatAsmName.Culture)
            {
                thisAsmName.Culture = null;
            }
        }

        public static void Intersect(this TypeData thisType, TypeData thatType)
        {
            thisType.Instance?.Intersect(thatType.Instance);
            thisType.Static?.Intersect(thatType.Static);
        }

        public static void Intersect(this MemberData thisMembers, MemberData thatMembers)
        {
            thisMembers.Events?.Intersect(thatMembers.Events);
            thisMembers.Fields?.Intersect(thatMembers.Fields);

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
                        thisIndexer.Intersect(thatIndexer);
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
                    property.Value.Intersect(thatMembers.Properties[property.Key]);
                }
            }

            thisMembers.NestedTypes?.Intersect(thatMembers.NestedTypes);
            foreach (KeyValuePair<string, TypeData> type in thisMembers.NestedTypes)
            {
                type.Value.Intersect(thatMembers.NestedTypes[type.Key]);
            }
        }

        public static void Intersect(this IndexerData thisIndexer, IndexerData thatIndexer)
        {
            thisIndexer.Accessors = thisIndexer.Accessors.Intersect(thatIndexer.Accessors).ToArray();
        }

        public static void Intersect(this PropertyData thisProperty, PropertyData thatProperty)
        {
            thisProperty.Accessors = thisProperty.Accessors.Intersect(thatProperty.Accessors).ToArray();
        }

        public static void Intersect(this ModuleData thisModule, ModuleData thatModule)
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
                    thisCmdlet.Value.Intersect(thatModule.Cmdlets[thisCmdlet.Key]);
                }
            }

            if (thisModule.Functions != null)
            {
                thisModule.Functions.Intersect(thatModule.Functions);

                foreach (KeyValuePair<string, FunctionData> thisFunction in thisModule.Functions)
                {
                    thisFunction.Value.Intersect(thatModule.Functions[thisFunction.Key]);
                }
            }
        }

        public static void Intersect(this CommandData thisCommand, CommandData thatCommand)
        {
            thisCommand.OutputType = thisCommand.OutputType?.Intersect(thatCommand.OutputType ?? Enumerable.Empty<string>()).ToArray();
            thisCommand.ParameterAliases?.Intersect(thatCommand?.ParameterAliases);
            thisCommand.ParameterSets = thisCommand.ParameterSets?.Intersect(thatCommand.ParameterSets ?? Enumerable.Empty<string>()).ToArray();

            if (thisCommand.Parameters != null)
            {
                thisCommand.Parameters?.Intersect(thatCommand.Parameters);

                foreach (KeyValuePair<string, ParameterData> parameter in thisCommand.Parameters)
                {
                    parameter.Value.Intersect(thatCommand.Parameters[parameter.Key]);
                }
            }
        }

        public static void Intersect(this ParameterData thisParam, ParameterData thatParam)
        {
            thisParam.ParameterSets?.Intersect(thatParam.ParameterSets);
        }

        public static void Union(this CompatibilityProfileData thisProfile, CompatibilityProfileData thatProfile)
        {
            // There's no simple solution to this
            // Users need to know what they're doing and add the platform in again manually
            thisProfile.Platforms = null;
        }

        private static void Intersect<K, V>(this IDictionary<K, V> thisDict, IDictionary<K, V> thatDict, IEqualityComparer<K> comparer = null)
        {
            // If the other dictionary is null, the best we can do is clear the current one -- it should also be set to null
            if (thatDict == null)
            {
                thisDict.Clear();
                return;
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
            var keysToRemove = comparer == null ? new HashSet<K>() : new HashSet<K>(comparer);
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
    }
}