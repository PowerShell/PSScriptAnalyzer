using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.CrossCompatibility
{
    public static class CompatibilityDataAssembler
    {
        private const BindingFlags StaticBinding = BindingFlags.Public | BindingFlags.Static;

        private const BindingFlags InstanceBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static readonly IEnumerable<string> s_specialMethodPrefixes = new [] {
            "get_",
            "set_",
            "add_",
            "remove_",
            "op_"
        };

        public static CompatibilityData Assemble(
            IEnumerable<Assembly> assemblies,
            IEnumerable<PSModuleInfo> modules,
            IDictionary<string, Type> typeAccelerators)
        {
            var moduleDict = new Dictionary<string, ModuleData>();
            foreach (PSModuleInfo module in modules)
            {
                moduleDict.Add(module.Name, AssembleModule(module));
            }

            return new CompatibilityData()
            {
                Types = AssembleAvailableTypes(assemblies, typeAccelerators),
                Modules = moduleDict
            };
        }

        public static ModuleData AssembleModule(PSModuleInfo moduleInfo)
        {
            var functions = new Dictionary<string, FunctionData>();
            foreach (KeyValuePair<string, FunctionInfo> funcEntry in moduleInfo.ExportedFunctions)
            {
                functions.Add(funcEntry.Key, AssembleFunction(funcEntry.Value));
            }

            var cmdlets = new Dictionary<string, CmdletData>();
            foreach (KeyValuePair<string, CmdletInfo> cmdletEntry in moduleInfo.ExportedCmdlets)
            {
                cmdlets.Add(cmdletEntry.Key, AssembleCmdlet(cmdletEntry.Value));
            }

            var aliases = new Dictionary<string, string>();
            foreach (AliasInfo alias in moduleInfo.ExportedAliases.Values)
            {
                aliases.Add(alias.Name, alias.ReferencedCommand.Name);
            }

            return new ModuleData()
            {
                Functions = functions.Any() ? functions : null,
                Cmdlets = cmdlets.Any() ? cmdlets : null,
                Aliases = aliases.Any() ? aliases : null,
                Variables = moduleInfo.ExportedVariables.Any() ? moduleInfo.ExportedVariables.Keys.ToArray() : null
            };
        }

        public static CmdletData AssembleCmdlet(CmdletInfo cmdlet)
        {
            List<string> outputTypes = null;
            if (cmdlet.OutputType != null)
            {
                outputTypes = new List<string>();
                foreach (PSTypeName type in cmdlet.OutputType)
                {
                    outputTypes.Add(type.Type.FullName);
                }
            }

            var parameterSets = new List<string>();
            foreach (CommandParameterSetInfo paramSet in cmdlet.ParameterSets)
            {
                parameterSets.Add(paramSet.Name);
            }

            var parameters = new Dictionary<string, ParameterData>();
            var aliases = new Dictionary<string, string>();
            if (cmdlet.Parameters != null)
            {
                foreach (KeyValuePair<string, ParameterMetadata> param in cmdlet.Parameters)
                {
                    parameters.Add(param.Key, AssembleParameter(param.Value));
                    if (param.Value.Aliases.Any())
                    {
                        foreach (string alias in param.Value.Aliases)
                        {
                            aliases.Add(alias, param.Key);
                        }
                    }
                }
            }

            return new CmdletData()
            {
                OutputType = outputTypes.ToArray(),
                ParameterSets = parameterSets.ToArray(),
                Parameters = parameters.Any() ? parameters : null,
                ParameterAliases = aliases.Any() ? aliases : null
            };
        }

        public static FunctionData AssembleFunction(FunctionInfo function)
        {
            List<string> outputTypes = null;
            if (function.OutputType != null)
            {
                outputTypes = new List<string>();
                foreach (PSTypeName type in function.OutputType)
                {
                    outputTypes.Add(type.Type.FullName);
                }
            }

            var parameterSets = new List<string>();
            foreach (CommandParameterSetInfo paramSet in function.ParameterSets)
            {
                parameterSets.Add(paramSet.Name);
            }

            var parameters = new Dictionary<string, ParameterData>();
            var aliases = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ParameterMetadata> param in function.Parameters)
            {
                parameters.Add(param.Key, AssembleParameter(param.Value));
                if (param.Value.Aliases.Any())
                {
                    foreach (string alias in param.Value.Aliases)
                    {
                        aliases.Add(alias, param.Key);
                    }
                }
            }

            return new FunctionData()
            {
                OutputType = outputTypes.ToArray(),
                ParameterSets = parameterSets.ToArray(),
                Parameters = parameters,
                ParameterAliases = aliases
            };
        }

        public static ParameterData AssembleParameter(ParameterMetadata parameter)
        {
            var parameterSets = new Dictionary<string, ParameterSetData>();
            foreach (KeyValuePair<string, ParameterSetMetadata> parameterSet in parameter.ParameterSets)
            {
                parameterSets.Add(parameterSet.Key, AssembleParameterSet(parameterSet.Value));
            }

            return new ParameterData()
            {
                Type = parameter.ParameterType.FullName,
                ParameterSets = parameterSets
            };
        }

        public static ParameterSetData AssembleParameterSet(ParameterSetMetadata paramSet)
        {
            var flags = new List<ParameterSetFlag>();

            if (paramSet.IsMandatory)
            {
                flags.Add(ParameterSetFlag.Mandatory);
            }

            if (paramSet.ValueFromPipeline)
            {
                flags.Add(ParameterSetFlag.ValueFromPipeline);
            }

            if (paramSet.ValueFromPipelineByPropertyName)
            {
                flags.Add(ParameterSetFlag.ValueFromPipelineByPropertyName);
            }

            if (paramSet.ValueFromRemainingArguments)
            {
                flags.Add(ParameterSetFlag.ValueFromRemainingArguments);
            }

            return new ParameterSetData()
            {
                Position = paramSet.Position,
                Flags = flags.ToArray()
            };
        }

        public static AvailableTypeData AssembleAvailableTypes(
            IEnumerable<Assembly> assemblies,
            IDictionary<string, Type> typeAccelerators)
        {
            var typeAcceleratorDict = new Dictionary<string, string>(typeAccelerators.Count);
            foreach (KeyValuePair<string, Type> typeAccelerator in typeAccelerators)
            {
                typeAcceleratorDict.Add(typeAccelerator.Key, typeAccelerator.Value.FullName);
            }


            var asms = new Dictionary<string, AssemblyData>();
            foreach (Assembly asm in assemblies)
            {
                KeyValuePair<string, AssemblyData> asmData = AssembleAssembly(asm);
                asms.Add(asmData.Key, asmData.Value);
            }

            return new AvailableTypeData()
            {
                TypeAccelerators = typeAcceleratorDict,
                Assemblies = asms
            };
        }

        public static KeyValuePair<string, AssemblyData> AssembleAssembly(Assembly asm)
        {
            AssemblyName asmName = asm.GetName();

            var asmNameData = new AssemblyNameData()
            {
                Name = asmName.Name,
                Version = asmName.Version,
                Culture = asmName.CultureName ?? "neutral",
                PublicKeyToken = asmName.GetPublicKeyToken(),
            };

            Type[] types = asm.GetTypes();
            Dictionary<string, IDictionary<string, TypeData>> namespacedTypes = null;
            if (types.Any())
            {
                namespacedTypes = new Dictionary<string, IDictionary<string, TypeData>>();
                foreach (Type type in asm.GetTypes())
                {
                    if (!type.IsPublic)
                    {
                        continue;
                    }

                    if (!namespacedTypes.ContainsKey(type.Namespace))
                    {
                        namespacedTypes.Add(type.Namespace, new Dictionary<string, TypeData>());
                    }

                    TypeData typeData = AssembleType(type);

                    namespacedTypes[type.Namespace][type.Name] = typeData;
                }
            }

            var asmData = new AssemblyData()
            {
                AssemblyName = asmNameData,
                Types = namespacedTypes
            };

            return new KeyValuePair<string, AssemblyData>(asmName.Name, asmData);
        }

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
                Instance = instanceMembers,
                Static = staticMembers
            };
        }

        public static MemberData AssembleMembers(Type type, BindingFlags memberBinding)
        {
            if ((memberBinding & BindingFlags.Instance) != 0 && (memberBinding & BindingFlags.Static) != 0)
            {
                throw new InvalidOperationException("Cannot assemble members for both static and instance members");
            }

            IEnumerable<MemberInfo> typeMembers = type.GetMembers(memberBinding)
                .Where(m => !HasSpecialMethodPrefix(m.Name));

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
                Events = events.Any() ? events.ToDictionary(e => e.Name, e => AssembleEvent(e)) : null,
                Fields = fields.Any() ? fields.ToDictionary(f => f.Name, f => AssembleField(f)) : null,
                Indexers = indexers.Any() ? indexers.Select(i => AssembleIndexer(i)).ToArray() : null,
                Methods = methods.Any() ? methods.ToDictionary(m => m.Key, m => AssembleMethod(m.Value)) : null,
                NestedTypes = nestedTypes.Any() ? nestedTypes.ToDictionary(t => t.Name, t => AssembleType(t)) : null,
                Properties = properties.Any() ? properties.ToDictionary(p => p.Name, p => AssembleProperty(p)) : null
            };
        }

        public static FieldData AssembleField(FieldInfo field)
        {
            return new FieldData()
            {
                Type = GetFullTypeName(field.FieldType)
            };
        }

        public static PropertyData AssembleProperty(PropertyInfo property)
        {
            return new PropertyData()
            {
                Accessors = GetAccessors(property),
                Type = GetFullTypeName(property.PropertyType)
            };
        }

        public static IndexerData AssembleIndexer(PropertyInfo indexer)
        {
            var paramTypes = new List<string>();
            foreach (ParameterInfo param in indexer.GetIndexParameters())
            {
                paramTypes.Add(GetFullTypeName(param.ParameterType));
            }

            return new IndexerData()
            {
                Accessors = GetAccessors(indexer),
                ItemType = GetFullTypeName(indexer.PropertyType),
                Parameters = paramTypes.ToArray()
            };
        }

        public static EventData AssembleEvent(EventInfo e)
        {
            return new EventData()
            {
                HandlerType = GetFullTypeName(e.EventHandlerType),
                IsMulticast = e.IsMulticast
            };
        }

        public static string[] AssembleConstructor(ConstructorInfo ctor)
        {
            var parameters = new List<string>();
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                parameters.Add(GetFullTypeName(param.ParameterType));
            }

            return parameters.ToArray();
        }

        public static MethodData AssembleMethod(List<MethodInfo> methodOverloads)
        {
            var overloads = new List<string[]>();
            foreach (MethodInfo overload in methodOverloads)
            {
                var parameters = new List<string>();
                foreach (ParameterInfo param in overload.GetParameters())
                {
                    parameters.Add(GetFullTypeName(param.ParameterType));
                }
                overloads.Add(parameters.ToArray());
            }

            return new MethodData()
            {
                ReturnType = GetFullTypeName(methodOverloads[0].ReturnType),
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

        private static bool HasSpecialMethodPrefix(string methodName)
        {
            foreach (string prefix in s_specialMethodPrefixes)
            {
                if (methodName.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetFullTypeName(Type type)
        {
            if (TryGetGenericParameterTypeName(type, out string genericTypeParamName))
            {
                return genericTypeParamName;
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var sb = new StringBuilder(type.Namespace).Append('.');

            if (!type.IsNested)
            {
                int backtickIdx = type.Name.IndexOf('`');
                sb.Append(type.Name.Substring(0, backtickIdx));
            }
            else
            {
                // Run up to the outermost type
                Type currType = type;
                var typePath = new Stack<Type>();
                do
                {
                    typePath.Push(currType);
                    currType = currType.DeclaringType;
                }
                while (currType != null);

                // Now unspool back down to the base type
                while (typePath.Any())
                {
                    Type innerType = typePath.Pop();

                    int backtickIdx = innerType.Name.IndexOf('`');
                    if (backtickIdx > 0)
                    {
                        sb.Append(innerType.Name.Substring(0, backtickIdx));
                    }
                    else
                    {
                        sb.Append(innerType.Name);
                    }

                    if (typePath.Any())
                    {
                        sb.Append('+');
                    }
                }
            }

            sb.Append('[');

            Type[] typeParameters = type.GetGenericArguments();
            for (int i = 0; i < typeParameters.Length; i++)
            {
                sb.Append(GetFullTypeName(typeParameters[i]));

                if (i < typeParameters.Length - 1)
                {
                    sb.Append(',');
                }
            }

            sb.Append(']');

            return sb.ToString();
        }
        
        private static bool TryGetGenericParameterTypeName(Type t, out string name)
        {
            if (t.IsGenericParameter)
            {
                string typeParamName = t.Name;
                name = new StringBuilder(typeParamName.Length + 2)
                    .Append('<')
                    .Append(typeParamName)
                    .Append('>')
                    .ToString();
                return true;
            }

            if (t.IsByRef && TryGetGenericParameterTypeName(t.GetElementType(), out string refElementName))
            {
                name = refElementName + '&';
                return true;
            }

            if (t.IsArray && TryGetGenericParameterTypeName(t.GetElementType(), out string arrElementName))
            {
                name = arrElementName + "[]";
                return true;
            }

            name = null;
            return false;
        }
    }
}