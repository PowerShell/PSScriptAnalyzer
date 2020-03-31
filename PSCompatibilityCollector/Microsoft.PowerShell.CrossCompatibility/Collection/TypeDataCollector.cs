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
using Microsoft.PowerShell.CrossCompatibility.Utility;

using SMA = System.Management.Automation;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    /// <summary>
    /// Assembles loaded type data from a list of assemblies.
    /// </summary>
    public class TypeDataCollector
    {
        /// <summary>
        /// Builds a TypeDataCollector objects using the configured settings.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Assemblies on paths starting with these prefixes will be excluded
            /// when collecting .NET type information.
            /// </summary>
            public IReadOnlyCollection<string> ExcludedAssemblyPathPrefixes { get; set; }

            /// <summary>
            /// Build the configured TypeDataCollector object.
            /// </summary>
            /// <returns>The constructed TypeDataCollector object.</returns>
            public TypeDataCollector Build(string psHomePath)
            {
                return new TypeDataCollector(psHomePath, ExcludedAssemblyPathPrefixes);
            }
        }

        // Binding flags for static type members
        private const BindingFlags StaticBinding = BindingFlags.Public | BindingFlags.Static;

        // Binding flags for instance type members, note FlattenHierarchy
        private const BindingFlags InstanceBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static readonly Assembly s_executingAssembly = Assembly.GetExecutingAssembly();

        private readonly IReadOnlyCollection<string> _excludedAssemblyPathPrefixes;

        private readonly string _psHomePath;

        private TypeDataCollector(
            string psHomePath,
            IReadOnlyCollection<string> excludedAssemblyPathPrefixes)
        {
            _psHomePath = psHomePath;
            _excludedAssemblyPathPrefixes = excludedAssemblyPathPrefixes;
        }

        /// <summary>
        /// Get .NET type data from the current session.
        /// </summary>
        /// <param name="errors">An enumeration of any errors encountered.</param>
        /// <returns>A data object describing the assemblies and PowerShell type accelerators available.</returns>
        public AvailableTypeData GetAvailableTypeData(out IEnumerable<CompatibilityAnalysisException> errors)
        {
            // PS 6+ behaves as if assemblies in PSHOME are already loaded when they aren't
            // so we must load them pre-emptively to capture the correct behavior
#if CoreCLR
            List<CompatibilityAnalysisException> psHomeLoadErrors = new List<CompatibilityAnalysisException>();
            foreach (string dllPath in Directory.GetFiles(_psHomePath))
            {
                if (!string.Equals(Path.GetExtension(dllPath), ".dll"))
                {
                    continue;
                }

                try
                {
                    Assembly.LoadFrom(dllPath);
                }
                catch (Exception e)
                {
                    psHomeLoadErrors.Add(new CompatibilityAnalysisException($"Unable to load PSHOME DLL at path '{dllPath}'", e));
                }
            }
#endif

            IReadOnlyDictionary<string, Type> typeAccelerators = GetTypeAccelerators();
            IEnumerable<Assembly> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

#if !CoreCLR
            return AssembleAvailableTypes(loadedAssemblies, typeAccelerators, out errors);
#else

            AvailableTypeData typeData = AssembleAvailableTypes(loadedAssemblies, typeAccelerators, out IEnumerable<CompatibilityAnalysisException> typeCollectionErrors);

            if (psHomeLoadErrors.Count > 0)
            {
                psHomeLoadErrors.AddRange(typeCollectionErrors);
                errors = psHomeLoadErrors;
            }
            else
            {
                errors = typeCollectionErrors;
            }

            return typeData;
#endif
        }

        /// <summary>
        /// Get the lookup table of PowerShell type accelerators in the current session.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Type> GetTypeAccelerators()
        {
            return (Dictionary<string, Type>)typeof(PSObject).Assembly
                .GetType("System.Management.Automation.TypeAccelerators")
                .GetMethod("get_Get")
                .Invoke(null, new object[0]);
        }

        /// <summary>
        /// Collate type data from assemblies into an AvailableTypeData object.
        /// </summary>
        /// <param name="assemblies">the assemblies to collate data from.</param>
        /// <param name="typeAccelerators">lookup table of PowerShell type accelerators.</param>
        /// <returns>an object describing all the available types from the given assemblies.</returns>
        public AvailableTypeData AssembleAvailableTypes(
            IEnumerable<Assembly> assemblies,
            IReadOnlyDictionary<string, Type> typeAccelerators,
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
                // Skip over this 
                if (asm == s_executingAssembly
                    || asm.IsDynamic
                    || string.IsNullOrEmpty(asm.Location))
                {
                    continue;
                }

                if (_excludedAssemblyPathPrefixes != null
                    && IsAssemblyPathExcluded(asm.Location))
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
                    try
                    {
                        asms.Add(asmData.Key, asmData.Value);
                    }
                    catch (ArgumentException e)
                    {
                        // We don't have a way in the schema for two assemblies with the same name, so we just keep the first
                        // This is not really valid and we should update the schema to subkey the version
                        errAcc.Add(new CompatibilityAnalysisException($"Found duplicate assemblies with name {asmData.Key}. Kept the first one.", e));
                    }
                }
                catch (Exception e) when (e is ReflectionTypeLoadException || e is FileNotFoundException)
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
        public KeyValuePair<string, AssemblyData> AssembleAssembly(Assembly asm)
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
            if (types.Length > 0)
            {
                namespacedTypes = new JsonDictionary<string, JsonDictionary<string, TypeData>>();
                foreach (Type type in types)
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
                        namespacedTypes.Add(typeNamespace, typeDictionary);
                    }

                    TypeData typeData;
                    try
                    {
                        typeData = AssembleType(type);
                    }
                    catch (Exception e) when (e is ReflectionTypeLoadException || e is FileNotFoundException)
                    {
                        continue;
                    }

                    typeDictionary[type.Name] = typeData;
                }
            }

            var asmData = new AssemblyData()
            {
                AssemblyName = asmNameData,
                Types = namespacedTypes != null && namespacedTypes.Count > 0 ? namespacedTypes : null,
            };

            return new KeyValuePair<string, AssemblyData>(asmName.Name, asmData);
        }

        /// <summary>
        /// Collate the data around a type.
        /// </summary>
        /// <param name="type">The type to assemble data for.</param>
        /// <returns>An object summarizing the type and its members.</returns>
        public TypeData AssembleType(Type type)
        {
            MemberData instanceMembers = AssembleMembers(type, InstanceBinding);
            MemberData staticMembers = AssembleMembers(type, StaticBinding);

            return new TypeData()
            {
                IsEnum = type.IsEnum,
                Instance = instanceMembers,
                Static = staticMembers
            };
        }

        private MemberData AssembleMembers(Type type, BindingFlags memberBinding)
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

            var constructors = new List<string[]>();
            var fields = new JsonDictionary<string, FieldData>();
            var properties = new JsonDictionary<string, PropertyData>();
            var indexers = new List<IndexerData>();
            var methods = new Dictionary<string, List<MethodInfo>>();
            var events = new JsonDictionary<string, EventData>();
            var nestedTypes = new JsonDictionary<string, TypeData>();

            foreach (MemberInfo member in typeMembers)
            {
                switch (member)
                {
                    case ConstructorInfo constructor:
                        constructors.Add(AssembleConstructor(constructor));
                        break;

                    case FieldInfo field:
                        fields.Add(field.Name, AssembleField(field));
                        break;

                    case EventInfo eventMember:
                        events.Add(eventMember.Name, AssembleEvent(eventMember));
                        break;

                    case TypeInfo nestedType:
                        nestedTypes.Add(nestedType.Name, AssembleType(nestedType));
                        break;

                    case PropertyInfo property:
                        if (IsIndexer(property))
                        {
                            indexers.Add(AssembleIndexer(property));
                            break;
                        }

                        properties.Add(property.Name, AssembleProperty(property));
                        break;
                    
                    case MethodInfo method:
                        if (!methods.TryGetValue(method.Name, out List<MethodInfo> overloads))
                        {
                            overloads = new List<MethodInfo>();
                            methods[method.Name] = overloads;
                        }
                        overloads.Add((MethodInfo)member);
                        break;
                }
            }

            bool anyConstructors = constructors.Count != 0;
            bool anyFields = fields.Count != 0;
            bool anyEvents = events.Count != 0;
            bool anyNestedTypes = nestedTypes.Count != 0;
            bool anyProperties = properties.Count != 0;
            bool anyIndexers = indexers.Count != 0;
            bool anyMethods = methods.Count != 0;

            if (!anyConstructors && !anyFields && !anyEvents && !anyNestedTypes && !anyProperties && !anyIndexers && !anyMethods)
            {
                return null;
            }

            // Process methods, since they had to be collected differently
            var methodDatas = new JsonDictionary<string, MethodData>();
            foreach (KeyValuePair<string, List<MethodInfo>> method in methods)
            {
                methodDatas[method.Key] = AssembleMethod(method.Value);
            }

            return new MemberData()
            {
                Constructors = anyConstructors ? constructors.ToArray() : null,
                Events = anyEvents ? events : null,
                Fields = anyFields ? fields : null,
                Indexers = anyIndexers ? indexers.ToArray() : null,
                Methods = anyMethods ? methodDatas : null,
                NestedTypes = anyNestedTypes ? nestedTypes : null,
                Properties = anyProperties ? properties : null
            };
        }

        private FieldData AssembleField(FieldInfo field)
        {
            return new FieldData()
            {
                Type = TypeNaming.GetFullTypeName(field.FieldType)
            };
        }

        private PropertyData AssembleProperty(PropertyInfo property)
        {
            return new PropertyData()
            {
                Accessors = GetAccessors(property),
                Type = TypeNaming.GetFullTypeName(property.PropertyType)
            };
        }

        private IndexerData AssembleIndexer(PropertyInfo indexer)
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

        private EventData AssembleEvent(EventInfo e)
        {
            return new EventData()
            {
                HandlerType = TypeNaming.GetFullTypeName(e.EventHandlerType),
                IsMulticast = e.IsMulticast
            };
        }

        private string[] AssembleConstructor(ConstructorInfo ctor)
        {
            var parameters = new List<string>();
            foreach (ParameterInfo param in ctor.GetParameters())
            {
                parameters.Add(TypeNaming.GetFullTypeName(param.ParameterType));
            }

            return parameters.ToArray();
        }

        private MethodData AssembleMethod(List<MethodInfo> methodOverloads)
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

        private bool IsAssemblyPathExcluded(string path)
        {
#if CoreCLR
            StringComparison stringComparisonType = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
#else
            StringComparison stringComparisonType = StringComparison.OrdinalIgnoreCase;
#endif
            foreach (string prefix in _excludedAssemblyPathPrefixes)
            {
                if (path.StartsWith(prefix, stringComparisonType))
                {
                    return true;
                }
            }
            return false;
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
