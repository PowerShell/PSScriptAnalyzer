// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// API for resolving PowerShell type names to .NET type names and vice versa.
    /// </summary>
    public static class TypeNaming
    {
        private static readonly ConcurrentDictionary<string, ITypeName> s_typeAcceleratorNameCache = new ConcurrentDictionary<string, ITypeName>();

        private static readonly IScriptExtent s_emptyExtent = (IScriptExtent)typeof(IScriptExtent).Assembly.GetTypes()
            .First(t => t.Name.Equals("EmptyScriptExtent"))
            .GetConstructor(new Type[0])
            .Invoke(new object[0]);

        /// <summary>
        /// Expand a non-generic, non-array, non-byref PowerShell-formatted type name to a full .NET type name.
        /// </summary>
        /// <param name="typeAccelerators">Lookup table of type accelerators available in the current target PowerShell runtime.</param>
        /// <param name="typeName">The PowerShell-format type name to expand.</param>
        /// <returns>The expanded .NET full typename.</returns>
        public static string ExpandSimpleTypeName(IReadOnlyDictionary<string, string> typeAccelerators, string typeName)
        {
            // Assumption that type accelerators do not contain '.'
            if (typeName.Contains("."))
            {
                return typeName;
            }

            if (typeAccelerators != null && typeAccelerators.TryGetValue(typeName, out string expandedTypeName))
            {
                return expandedTypeName;
            }

            return typeName;
        }

        public static string GetOuterMostTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ITypeName typeName)
        {
            switch (typeName)
            {
                case TypeName simpleTypeName:
                    return ExpandSimpleTypeName(typeAccelerators, simpleTypeName.FullName);

                case ArrayTypeName arrayTypeName:
                    return GetOuterMostTypeName(typeAccelerators, arrayTypeName.ElementType);

                case GenericTypeName genericTypeName:
                    return GetOuterMostTypeName(typeAccelerators, genericTypeName.TypeName);

                case ReflectionTypeName reflectionTypeName:
                    return GetFullTypeName(reflectionTypeName.GetReflectionType());

                default:
                    throw new ArgumentException($"{nameof(typeName)} is not a known instantiation of ITypeName. Type: {typeName.GetType()}");
            }
        }

        /// <summary>
        /// Get the expanded .NET type name from a PowerShell ITypeName object.
        /// </summary>
        /// <param name="typeAccelerators">The type accelerators available in the target PowerShell runtime.</param>
        /// <param name="typeName">The PowerShell ITypeName to expand.</param>
        /// <returns>The full .NET type name of the type.</returns>
        public static string GetCanonicalTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ITypeName typeName)
        {
            if (typeName is ReflectionTypeName reflectionTypeName)
            {
                return GetFullTypeName(reflectionTypeName.GetReflectionType());
            }

            return ExpandTypeName(typeAccelerators, typeName).FullName;
        }

        /// <summary>
        /// Get the full name of a .NET type, without assembly qualification of generics.
        /// </summary>
        /// <param name="type">The type to get the full name of.</param>
        /// <returns>The full, namespace-qualified name of the type, without assembly qualification.</returns>
        public static string GetFullTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Non-generic type names give their full names as something PowerShell can recognize
            if (!IsGeneric(type))
            {
                return type.FullName ?? type.Name;
            }

            if (type.IsArray)
            {
                return GetFullTypeName(type.GetElementType()) + "[]";
            }

            if (type.IsByRef)
            {
                return GetFullTypeName(type.GetElementType()) + "&";
            }

            // Uninstantiated generics also have PowerShell-parseable full names
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.All(ga => ga.IsGenericParameter))
            {
                if (type.FullName != null)
                {
                    return type.FullName;
                }

                return RemoveGenericParameters(type.ToString());
            }

            var sb = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append('[');

            int i = 0;
            for (; i < genericArguments.Length - 1; i++)
            {
                Type genericArg = genericArguments[i];
                if (!genericArg.IsGenericParameter)
                {
                    sb.Append(GetFullTypeName(genericArg));
                }
                sb.Append(',');
            }
            sb.Append(genericArguments[i]).Append(']');

            return sb.ToString();
        }

        /// <summary>
        /// Heuristic to check if a full .NET or PowerShell type name represents a generic.
        /// </summary>
        /// <param name="typeName">The full type name to check.</param>
        /// <returns>True if the type name represents a genric type, false otherwise.</returns>
        public static bool IsGenericName(string typeName)
        {
            return typeName.Contains("`");
        }

        /// <summary>
        /// Strip out the generic quantification from a .NET type name.
        /// For example "System.Collections.Generic.Dictionary`2" -> "System.Collections.Generic.Dictionary".
        /// </summary>
        /// <param name="typeName">The name of the type to strip.</param>
        /// <returns>The type name without its generic quantifiers.</returns>
        public static string StripGenericQuantifiers(string typeName)
        {
            var sb = new StringBuilder();
            int currSectionStart = 0;
            int i = 0;
            for (; i < typeName.Length; i++)
            {
                if (typeName[i] != '`')
                {
                    continue;
                }

                sb.Append(typeName.Substring(currSectionStart, i - currSectionStart));

                do
                {
                    i++;
                }
                while (i < typeName.Length && char.IsDigit(typeName[i]));

                currSectionStart = i;
            }
            if (currSectionStart < typeName.Length)
            {
                sb.Append(typeName.Substring(currSectionStart, i - currSectionStart));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Assemble the full name of a type given its simple name and namespace.
        /// </summary>
        /// <param name="nspace">The namespace of the type.</param>
        /// <param name="typeName">The simple name of the type.</param>
        /// <returns>The namespace-qualified full name of the type.</returns>
        public static string AssembleFullName(string nspace, string typeName)
        {
            if (string.IsNullOrEmpty(nspace))
            {
                return typeName;
            }

            return nspace + "." + typeName;
        }

        private static string RemoveGenericParameters(string typeName)
        {
            var sb = new StringBuilder();
            int lastOffset = 0;
            int i = 0;
            for (; i < typeName.Length; i++)
            {
                if (typeName[i] != '[')
                {
                    continue;
                }

                if (typeName[i + 1] == ']')
                {
                    continue;
                }

                sb.Append(typeName.Substring(lastOffset, i - lastOffset));
                i = typeName.IndexOf(']', lastOffset);
                lastOffset = i;
            }
            sb.Append(typeName.Substring(lastOffset + 1, i - lastOffset - 1));

            return sb.ToString();
        }

        private static bool IsGeneric(Type type)
        {
            if (type.IsGenericType)
            {
                return true;
            }

            if (type.IsArray || type.IsByRef || type.IsPointer)
            {
                return IsGeneric(type.GetElementType());
            }

            return false;
        }

        private static ITypeName ExpandTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ITypeName typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            switch (typeName)
            {
                case TypeName ordinaryTypeName:
                    return ExpandTypeName(typeAccelerators, ordinaryTypeName);

                case ArrayTypeName arrayTypeName:
                    return ExpandTypeName(typeAccelerators, arrayTypeName);

                case GenericTypeName genericTypeName:
                    return ExpandTypeName(typeAccelerators, genericTypeName);

                case ReflectionTypeName reflectionTypeName:
                    return ExpandTypeName(typeAccelerators, reflectionTypeName);

                default:
                    throw new ArgumentException($"{nameof(typeName)} is not a known instantiation of ITypeName. Type: {typeName.GetType()}");
            }
        }

        private static ITypeName ExpandTypeName(IReadOnlyDictionary<string, string> typeAccelerators, TypeName typeName, int genericArgCount = 0)
        {
            if (genericArgCount > 0 && !typeName.FullName.Contains("`"))
            {
                string newTypeName = new StringBuilder(typeName.FullName).Append('`').Append(genericArgCount).ToString();
                return new TypeName(s_emptyExtent, newTypeName);
            }

            if (typeName.FullName.Contains("."))
            {
                return typeName;
            }

            if (s_typeAcceleratorNameCache.TryGetValue(typeName.FullName, out ITypeName expandedName))
            {
                return expandedName;
            }

            if (typeAccelerators.TryGetValue(typeName.FullName, out string expandedTypeName))
            {
                var newExpandedName = new TypeName(s_emptyExtent, expandedTypeName);
                s_typeAcceleratorNameCache[typeName.FullName] = newExpandedName;
                return newExpandedName;
            }

            if (Type.GetType(typeName.FullName, throwOnError: false, ignoreCase: true) == null)
            {
                string systemExpandedName = "System." + typeName.FullName;
                Type systemExpandedType = Type.GetType(systemExpandedName, throwOnError: false, ignoreCase: true);

                if (systemExpandedType != null)
                {
                    var newExpandedName = new TypeName(s_emptyExtent, GetFullTypeName(systemExpandedType));
                    s_typeAcceleratorNameCache[typeName.FullName] = newExpandedName;
                    return newExpandedName;
                }
            }

            return typeName;
        }

        private static ITypeName ExpandTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ArrayTypeName typeName)
        {
            ITypeName elementTypeName = ExpandTypeName(typeAccelerators, typeName.ElementType);

            if (elementTypeName == typeName.ElementType)
            {
                return typeName;
            }

            return new ArrayTypeName(s_emptyExtent, elementTypeName, typeName.Rank);
        }

        private static ITypeName ExpandTypeName(IReadOnlyDictionary<string, string> typeAccelerators, GenericTypeName typeName)
        {
            var genericArgs = new ITypeName[typeName.GenericArguments.Count];
            for (int i = 0; i < genericArgs.Length; i++)
            {
                genericArgs[i] = ExpandTypeName(typeAccelerators, typeName.GenericArguments[i]);
            }

            ITypeName expandedTypeName = ExpandTypeName(typeAccelerators, (TypeName)typeName.TypeName, genericArgCount: genericArgs.Length);

            bool canUseOldTypeName = expandedTypeName == typeName.TypeName;
            for (int i = 0; i < genericArgs.Length; i++)
            {
                if (!canUseOldTypeName)
                {
                    break;
                }

                canUseOldTypeName &= genericArgs[i] == typeName.GenericArguments[i];
            }

            if (canUseOldTypeName)
            {
                return typeName;
            }

            return new GenericTypeName(s_emptyExtent, expandedTypeName, genericArgs);
        }

        private static ITypeName ExpandTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ReflectionTypeName typeName)
        {
            return typeName;
        }
    }
}
