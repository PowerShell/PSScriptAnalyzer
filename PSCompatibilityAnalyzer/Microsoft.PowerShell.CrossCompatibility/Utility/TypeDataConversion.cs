// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Data.Modules;
using Microsoft.PowerShell.CrossCompatibility.Data.Types;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    /// <summary>
    /// Assembles loaded type data from a list of assemblies.
    /// </summary>
    public static class TypeDataConversion
    {
        // Binding flags for static type members
        private const BindingFlags StaticBinding = BindingFlags.Public | BindingFlags.Static;

        // Binding flags for instance type members, note FlattenHierarchy
        private const BindingFlags InstanceBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        /// <summary>
        /// Collate type data from assemblies into an AvailableTypeData object.
        /// </summary>
        /// <param name="assemblies">the assemblies to collate data from.</param>
        /// <param name="typeAccelerators">lookup table of PowerShell type accelerators.</param>
        /// <returns>an object describing all the available types from the given assemblies.</returns>
        public static AvailableTypeData AssembleAvailableTypes(
            IEnumerable<Assembly> assemblies,
            IDictionary<string, Type> typeAccelerators,
            out IEnumerable<CompatibilityAnalysisException> errors)
        {
            var errAcc = new List<CompatibilityAnalysisException>();
            var typeAcceleratorDict = new JsonCaseInsensitiveStringDictionary<TypeAcceleratorData>(typeAccelerators.Count);
            foreach (KeyValuePair<string, Type> typeAccelerator in typeAccelerators)
            {
                var ta = new TypeAcceleratorData()
                {
                    Assembly = typeAccelerator.Value.Assembly.GetName().Name,
                    Type = typeAccelerator.Value.FullName
                };

                typeAcceleratorDict.Add(typeAccelerator.Key, ta);
            }

            var asms = new JsonDictionary<string, AssemblyData>();
            foreach (Assembly asm in assemblies)
            {
                // Don't want to include this module or assembly in the output
                if (Assembly.GetCallingAssembly() == asm)
                {
                    continue;
                }

                try
                {
                    // First check whether an assembly with this name already exists
                    // Only replace it if the current one is newer
                    AssemblyName asmName = asm.GetName();
                    if (asms.TryGetValue(asmName.Name, out AssemblyData currentAssemblyData)
                        && asmName.Version < currentAssemblyData.AssemblyName.Version)
                    {
                        continue;
                    }

                    KeyValuePair<string, AssemblyData> asmData = AssembleAssembly(asm);
                    asms[asmData.Key] = asmData.Value;
                }
                catch (ReflectionTypeLoadException e)
                {
                    errAcc.Add(new CompatibilityAnalysisException($"Failed to load assembly '{asm.GetName().FullName}'", e));
                }
            }

            errors = errAcc;
            return new AvailableTypeData()
            {
                TypeAccelerators = typeAcceleratorDict,
                Assemblies = asms
            };
        }

        /// <summary>
        /// Collate an AssemblyData object for a single assembly.
        /// </summary>
        /// <param name="asm">the assembly to collect data on.</param>
        /// <returns>A pair of the name and data of the given assembly.</returns>
        public static KeyValuePair<string, AssemblyData> AssembleAssembly(Assembly asm)
        {
            AssemblyName asmName = asm.GetName();

            var asmNameData = new AssemblyNameData()
            {
                Name = asmName.Name,
                Version = asmName.Version,
                Culture = string.IsNullOrEmpty(asmName.CultureName) ? null : asmName.CultureName,
                PublicKeyToken = asmName.GetPublicKeyToken(),
            };

            Type[] types = asm.GetTypes();
            JsonDictionary<string, JsonDictionary<string, TypeData>> namespacedTypes = null;
            if (types.Any())
            {
                namespacedTypes = new JsonDictionary<string, JsonDictionary<string, TypeData>>();
                foreach (Type type in asm.GetTypes())
                {
                    if (!type.IsPublic)
                    {
                        continue;
                    }

                    // Some types don't have a namespace, but we still want to file them
                    string typeNamespace = type.Namespace ?? "";

                    if (!namespacedTypes.TryGetValue(typeNamespace, out JsonDictionary<string, TypeData> typeDictionary))
                    {
                        typeDictionary = new JsonDictionary<string, TypeData>();
                    }

                    TypeData typeData = AssembleType(type);

                    typeDictionary[type.Name] = typeData;
                }
            }

            var asmData = new AssemblyData()
            {
                AssemblyName = asmNameData,
                Types = namespacedTypes
            };

            return new KeyValuePair<string, AssemblyData>(asmName.Name, asmData);
        }

        /// <summary>
        /// Collate the data around a type.
        /// </summary>
        /// <param name="type">The type to assemble data for.</param>
        /// <returns>An object summarizing the type and its members.</returns>
        public static TypeData AssembleType(Type type)
        {
            MemberData instanceMembers = AssembleMembers(type, InstanceBinding);
            MemberData staticMembers = AssembleMembers(type, StaticBinding);

            if (instanceMembers.Constructors == null
                && instanceMembers.Events == null
                && instanceMembers.Fields == null
                && instanceMembers.Indexers == null
                && instanceMembers.Methods == null
                && instanceMembers.NestedTypes == null
                && instanceMembers.Properties == null)
            {
                instanceMembers = null;
            }

            if (staticMembers.Constructors == null
                && staticMembers.Events == null
                && staticMembers.Fields == null
                && staticMembers.Indexers == null
                && staticMembers.Methods == null
                && staticMembers.NestedTypes == null
                && staticMembers.Properties == null)
            {
                staticMembers = null;
            }

            return new TypeData()
            {
                IsEnum = type.IsEnum,
                Instance = instanceMembers,
                Static = staticMembers
            };
        }

        private static MemberData AssembleMembers(Type type, BindingFlags memberBinding)
        {
            if ((memberBinding & BindingFlags.Instance) != 0 && (memberBinding & BindingFlags.Static) != 0)
            {
                throw new InvalidOperationException("Cannot assemble members for both static and instance members");
            }

            IEnumerable<MemberInfo> typeMembers = type.GetMembers(memberBinding)
                .Where(m => !HasSpecialMethodPrefix(m));

            // If we are dealing with instance members, we need to be careful about overrides
            if ((memberBinding & BindingFlags.Instance) != 0)
            {
                var members = new Dictionary<string, List<MemberInfo>>();
                foreach (MemberInfo memberInfo in typeMembers)
                {
                    if (!members.ContainsKey(memberInfo.Name))
                    {
                        members.Add(memberInfo.Name, new List<MemberInfo>() { memberInfo });
                        continue;
                    }

                    List<MemberInfo> existingMembers = members[memberInfo.Name];
                    switch (memberInfo.MemberType)
                    {
                        case MemberTypes.Field:
                        case MemberTypes.Event:
                        case MemberTypes.NestedType:
                            ReplaceWithMemberIfOverrides(existingMembers, memberInfo);
                            continue;

                        case MemberTypes.Property:
                            if (!IsIndexer((PropertyInfo)memberInfo))
                            {
                                ReplaceWithMemberIfOverrides(existingMembers, memberInfo);
                                continue;
                            }
                            InsertOrOverrideParameteredMember(existingMembers, memberInfo);
                            continue;

                        case MemberTypes.Constructor:
                        case MemberTypes.Method:
                            InsertOrOverrideParameteredMember(existingMembers, memberInfo);
                            continue;
                    }
                }
                typeMembers = members.Values.SelectMany(ms => ms);
            }

            var constructors = new List<ConstructorInfo>();
            var fields = new List<FieldInfo>();
            var properties = new List<PropertyInfo>();
            var indexers = new List<PropertyInfo>();
            var methods = new Dictionary<string, List<MethodInfo>>();
            var events = new List<EventInfo>();
            var nestedTypes = new List<TypeInfo>();

            foreach (MemberInfo member in typeMembers)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Constructor:
                        constructors.Add((ConstructorInfo)member);
                        break;

                    case MemberTypes.Field:
                        fields.Add((FieldInfo)member);
                        break;

                    case MemberTypes.Event:
                        events.Add((EventInfo)member);
                        break;

                    case MemberTypes.NestedType:
                        nestedTypes.Add((TypeInfo)member);
                        break;

                    case MemberTypes.Property:
                        var property = (PropertyInfo)member;
                        if (IsIndexer(property))
                        {
                            indexers.Add(property);
                            break;
                        }

                        properties.Add(property);
                        break;
                    
                    case MemberTypes.Method:
                        if (!methods.ContainsKey(member.Name))
                        {
                            methods.Add(member.Name, new List<MethodInfo>());
                        }
                        methods[member.Name].Add((MethodInfo)member);
                        break;
                }
            }

            return new MemberData()
            {
                Constructors = constructors.Any() ? constructors.Select(c => AssembleConstructor(c)).ToArray() : null,
                Events = events.Any() ? new JsonDictionary<string, EventData>(events.ToDictionary(e => e.Name, e => AssembleEvent(e))) : null,
                Fields = fields.Any() ? new JsonDictionary<string, FieldData>(fields.ToDictionary(f => f.Name, f => AssembleField(f))) : null,
                Indexers = indexers.Any() ? indexers.Select(i => AssembleIndexer(i)).ToArray() : null,
                Methods = methods.Any() ? new JsonDictionary<string, MethodData>(methods.ToDictionary(m => m.Key, m => AssembleMethod(m.Value))) : null,
                NestedTypes = nestedTypes.Any() ? new JsonDictionary<string, TypeData>(nestedTypes.ToDictionary(t => t.Name, t => AssembleType(t))) : null,
                Properties = properties.Any() ? new JsonDictionary<string, PropertyData>(properties.ToDictionary(p => p.Name, p => AssembleProperty(p))) : null
            };
        }

        private static FieldData AssembleField(FieldInfo field)
        {
            return new FieldData()
            {
                Type = TypeNaming.GetFullTypeName(field.FieldType)
            };
        }

        private static PropertyData AssembleProperty(PropertyInfo property)
        {
            return new PropertyData()
            {
                Accessors = GetAccessors(property),
                Type = TypeNaming.GetFullTypeName(property.PropertyType)
            };
        }

        private static IndexerData AssembleIndexer(PropertyInfo indexer)
        {
            var paramTypes = new List<string>();
            foreach (ParameterInfo param in indexer.GetIndexParameters())
            {
                paramTypes.Add(TypeNaming.GetFullTypeName(param.ParameterType));
            }

            return new IndexerData()
            {
                Accessors = GetAccessors(indexer),
                ItemType = TypeNaming.GetFullTypeName(indexer.PropertyType),
                Parameters = paramTypes.ToArray()
            };
        }

        private static EventData AssembleEvent(EventInfo e)
        {
            return new EventData()
            {
                HandlerType = TypeNaming.GetFullTypeName(e.EventHandlerType),
                IsMulticast = e.IsMulticast
            };
        }

        private static string[] AssembleConstructor(ConstructorInfo ctor)
        {
            var parameters = new List<string>();
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                parameters.Add(TypeNaming.GetFullTypeName(param.ParameterType));
            }

            return parameters.ToArray();
        }

        private static MethodData AssembleMethod(List<MethodInfo> methodOverloads)
        {
            var overloads = new List<string[]>();
            foreach (MethodInfo overload in methodOverloads)
            {
                var parameters = new List<string>();
                foreach (ParameterInfo param in overload.GetParameters())
                {
                    parameters.Add(TypeNaming.GetFullTypeName(param.ParameterType));
                }
                overloads.Add(parameters.ToArray());
            }

            return new MethodData()
            {
                ReturnType = TypeNaming.GetFullTypeName(methodOverloads[0].ReturnType),
                OverloadParameters = overloads.ToArray()
            };
        }

        private static void ReplaceWithMemberIfOverrides(List<MemberInfo> existingMembers, MemberInfo newMember)
        {
            // If the existing members belong to a subclass, they override
            if (existingMembers[0].DeclaringType.IsSubclassOf(newMember.DeclaringType))
            {
                return;
            }

            // Otherwise, assume the reverse
            existingMembers.Clear();
            existingMembers.Add(newMember);
        }

        private static void InsertOrOverrideParameteredMember(List<MemberInfo> existingMembers, MemberInfo newMember)
        {
            // First check if the new member is a different type, since it may just override all existing
            if (existingMembers[0].MemberType != newMember.MemberType)
            {
                if (existingMembers[0].DeclaringType.IsSubclassOf(newMember.DeclaringType))
                {
                    // The existing members are the overriding ones, so there is nothing to do
                    return;
                }

                // Assume the new member is the overriding one
                existingMembers.Clear();
                existingMembers.Add(newMember);
                return;
            }

            ParameterInfo[] newParameterSet;
            ParameterInfo[][] existingParameterSets;
            switch (existingMembers[0].MemberType)
            {
                case MemberTypes.Method:
                    newParameterSet = ((MethodInfo)newMember).GetParameters();
                    existingParameterSets = existingMembers.Select(m => ((MethodInfo)m).GetParameters()).ToArray();
                    break;

                case MemberTypes.Constructor:
                    newParameterSet = ((ConstructorInfo)newMember).GetParameters();
                    existingParameterSets = existingMembers.Select(c => ((ConstructorInfo)c).GetParameters()).ToArray();
                    break;

                case MemberTypes.Property:
                    newParameterSet = ((PropertyInfo)newMember).GetIndexParameters();
                    existingParameterSets = existingMembers.Select(p => ((PropertyInfo)p).GetIndexParameters()).ToArray();
                    break;

                default:
                    throw new InvalidOperationException($"Cannot do perform parameter override for member type {existingMembers[0].MemberType}");
            }

            for (int i = 0; i < existingMembers.Count; i++)
            {
                ParameterInfo[] currParamSet = existingParameterSets[i];
                if (newParameterSet.Length != currParamSet.Length)
                {
                    continue;
                }

                for (int j = 0; j < newParameterSet.Length; j++)
                {
                    if (newParameterSet[j].ParameterType != currParamSet[j].ParameterType)
                    {
                        continue;
                    }

                    // At this point, we know one member overrides the other.
                    // We just need to determine which
                    if (existingMembers[i].DeclaringType.IsSubclassOf(newMember.DeclaringType))
                    {
                        // The new member was already overridden so there is nothing more to do
                        return;
                    }

                    // We assume the new member must override the existing one
                    existingMembers[i] = newMember;
                    return;
                }
            }

            // If we found no existing members with conflicting parameters,
            // there's no need to override and we can just add the new overload
            existingMembers.Add(newMember);
        }
        
        private static AccessorType[] GetAccessors(PropertyInfo propertyInfo)
        {
            var accessors = new List<AccessorType>();

            if (propertyInfo.GetMethod?.IsPublic ?? false)
            {
                accessors.Add(AccessorType.Get);
            }

            if (propertyInfo.SetMethod?.IsPublic ?? false)
            {
                accessors.Add(AccessorType.Set);
            }

            return accessors.ToArray();
        }

        private static bool IsIndexer(PropertyInfo property)
        {
            ParameterInfo[] parameters = property.GetIndexParameters();

            return parameters != null && parameters.Any();
        }

        private static bool HasSpecialMethodPrefix(MemberInfo member)
        {
            switch (member)
            {
                case MethodInfo method:
                    return method.IsSpecialName;

                default:
                    return false;
            }
        }
    }
}
