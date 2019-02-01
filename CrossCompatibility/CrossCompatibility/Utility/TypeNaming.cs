using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public static class TypeNaming
    {
        private static readonly ConcurrentDictionary<string, ITypeName> s_typeAcceleratorNameCache = new ConcurrentDictionary<string, ITypeName>();

        private static readonly IScriptExtent s_emptyExtent = (IScriptExtent)typeof(IScriptExtent).Assembly.GetTypes()
            .First(t => t.Name.Equals("EmptyScriptExtent"))
            .GetConstructor(new Type[0])
            .Invoke(new object[0]);

        public static string GetCanonicalTypeName(IReadOnlyDictionary<string, string> typeAccelerators, ITypeName typeName)
        {
            if (typeName is ReflectionTypeName reflectionTypeName)
            {
                return GetFullTypeName(reflectionTypeName.GetReflectionType());
            }

            return ExpandTypeName(typeAccelerators, typeName).FullName;
        }

        public static string GetFullTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Non-generic type names give their full names as something PowerShell can recognize
            if (!IsGeneric(type))
            {
                return type.FullName;
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

        public static bool IsGenericName(string typeName)
        {
            return typeName.Contains('`');
        }

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
            if (genericArgCount > 0 && !typeName.FullName.Contains('`'))
            {
                string newTypeName = new StringBuilder(typeName.FullName).Append('`').Append(genericArgCount).ToString();
                return new TypeName(s_emptyExtent, newTypeName);
            }

            if (typeName.FullName.Contains('.'))
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