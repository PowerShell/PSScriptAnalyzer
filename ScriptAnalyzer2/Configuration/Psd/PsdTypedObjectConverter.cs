using Microsoft.PowerShell.ScriptAnalyzer.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration.Psd
{
    public abstract class PsdTypeConverter
    {
        public abstract bool CanConvert(Type type);

        public abstract object ConvertPsdType(Type type, ExpressionAst psdAst);
    }

    public abstract class PsdTypeConverter<TTarget> : PsdTypeConverter
    {
        public abstract TTarget ConvertPsdType(ExpressionAst psdAst);

        public override bool CanConvert(Type type)
        {
            return type == typeof(TTarget);
        }

        public override object ConvertPsdType(Type type, ExpressionAst psdAst)
        {
            return ConvertPsdType(psdAst);
        }
    }

    public class PsdTypedObjectConverter
    {
        private readonly PsdDataParser _psdDataParser;

        private readonly IReadOnlyList<PsdTypeConverter> _converters;

        private readonly Dictionary<Type, PsdTypeConverter> _converterCache;

        private readonly Dictionary<Type, Dictionary<string, object>> _enumMembers;

        public PsdTypedObjectConverter(IReadOnlyList<PsdTypeConverter> converters)
        {
            _converters = converters;
            _psdDataParser = new PsdDataParser();
            _converterCache = new Dictionary<Type, PsdTypeConverter>();
            _enumMembers = new Dictionary<Type, Dictionary<string, object>>();
        }

        public PsdTypedObjectConverter() : this(converters: null)
        {
        }

        public TTarget Convert<TTarget>(string str)
        {
            return (TTarget)Convert(typeof(TTarget), str);
        }

        public object Convert(Type type, string str)
        {
            ExpressionAst expressionAst = PowerShellParsing.ParseExpressionFromInput(str);
            return Convert(type, expressionAst);
        }

        public TTarget Convert<TTarget>(ExpressionAst ast)
        {
            return (TTarget)Convert(typeof(TTarget), ast);
        }

        public object Convert(Type type, ExpressionAst ast)
        {
            if (type == typeof(ExpressionAst))
            {
                return ast;
            }

            if (type == typeof(object))
            {
                return _psdDataParser.ConvertAstValue(ast);
            }

            object conversionResult;

            if (TryCustomConversion(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            if (TryEnumConversion(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            if (TryConvertVariableExpression(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            // Check primitive types the fast way
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return ConvertString(ast);

                case TypeCode.DateTime:
                    return ConvertDateTime(ast);

                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new ArgumentException($"Type '{type.FullName}' has invalid type code '{Type.GetTypeCode(type)}'");

                case TypeCode.Boolean:
                    throw new ArgumentException($"Ast '{ast.Extent.Text}' cannot be cast to boolean type");
            }

            if (TryConvertNumber(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            if (TryConvertNullable(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            if (TryConvertDictionary(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            if (TryConvertEnumerable(type, ast, out conversionResult))
            {
                return conversionResult;
            }

            return ConvertPoco(type, ast);
        }

        private bool TryEnumConversion(Type target, ExpressionAst ast, out object result)
        {
            if (!target.IsEnum)
            {
                result = null;
                return false;
            }

            string str = ConvertString(ast);

            if (!_enumMembers.TryGetValue(target, out Dictionary<string, object> enumValues))
            {
                enumValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (FieldInfo enumMember in target.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var enumMemberAttribute = enumMember.GetCustomAttribute<EnumMemberAttribute>();
                    if (enumMemberAttribute != null)
                    {
                        enumValues[enumMemberAttribute.Value ?? enumMember.Name] = enumMember.GetValue(null);
                    }
                }

                // If no members have the [EnumMember] attribute, we include all of them
                if (enumValues.Count == 0)
                {
                    foreach (FieldInfo enumMember in target.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        enumValues[enumMember.Name] = enumMember.GetValue(null);
                    }
                }

                _enumMembers[target] = enumValues;
            }

            if (!enumValues.TryGetValue(str, out result))
            {
                throw new ArgumentException($"Enum type '{target.FullName}' does not define a member '{str}'");
            }

            return true;
        }

        private bool TryConvertNullable(Type target, ExpressionAst ast, out object result)
        {
            if (!target.IsGenericType
                || target.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                result = null;
                return false;
            }

            Type innerType = target.GetGenericArguments()[0];
            object innerValue = Convert(innerType, ast);

            result = typeof(Nullable<>)
                .MakeGenericType(innerType)
                .GetConstructor(new Type[] { innerType })
                .Invoke(new object[] { innerValue });
            return true;
        }

        private bool TryCustomConversion(Type target, ExpressionAst ast, out object result)
        {
            if (_converterCache.TryGetValue(target, out PsdTypeConverter cachedConverter))
            {
                result = cachedConverter.ConvertPsdType(target, ast);
                return true;
            }

            if (_converters != null)
            {
                foreach (PsdTypeConverter converter in _converters)
                {
                    if (converter.CanConvert(target))
                    {
                        _converterCache[target] = converter;
                        result = converter.ConvertPsdType(target, ast);
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        private Dictionary<string, ExpressionAst> GetHashtableDict(Type target, ExpressionAst ast)
        {
            if (!(ast is HashtableAst hashtableAst))
            {
                throw CreateConversionMismatchException(ast, target);
            }

            var hashtableFields = new Dictionary<string, ExpressionAst>(StringComparer.OrdinalIgnoreCase);
            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                string key = (string)_psdDataParser.ConvertAstValue(entry.Item1);
                hashtableFields[key] = GetExpressionFromStatementAst(entry.Item2);
            }

            return hashtableFields;
        }

        private ConstructorInfo GetDesignatedConstructor(Type target)
        {
            // Work out if we'll be able to construct this type
            ConstructorInfo[] constructors = target.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            ConstructorInfo designatedConstructor = null;
            if (constructors.Length == 1)
            {
                designatedConstructor = constructors[0];
            }
            else
            {
                foreach (ConstructorInfo ctorInfo in target.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (ctorInfo.GetParameters().Length == 0)
                    {
                        designatedConstructor = ctorInfo;
                        continue;
                    }

                    if (ctorInfo.GetCustomAttribute<JsonConstructorAttribute>() != null)
                    {
                        designatedConstructor = ctorInfo;
                        break;
                    }
                }
            }

            if (designatedConstructor == null)
            {
                throw new ArgumentException($"Unable to instantiate type '{target.FullName}': no constructor available for instantiation");
            }

            return designatedConstructor;
        }

        private Dictionary<string, SerializedProperty> GetMembersToInstantiate(Type target)
        {
            var jsonObjectAttribute = target.GetCustomAttribute<JsonObjectAttribute>();

            var memberSerialization = MemberSerialization.OptOut;
            if (jsonObjectAttribute != null)
            {
                memberSerialization = jsonObjectAttribute.MemberSerialization;
            }
            else if (target.GetCustomAttribute<DataContractAttribute>() != null)
            {
                memberSerialization = MemberSerialization.OptIn;
            }

            var membersToInstantiate = new Dictionary<string, SerializedProperty>(StringComparer.OrdinalIgnoreCase);
            AddFieldsToInstantiate(target, memberSerialization, jsonObjectAttribute, membersToInstantiate);
            AddPropertiesToInstantiate(target, memberSerialization, jsonObjectAttribute, membersToInstantiate);
            return membersToInstantiate;
        }

        private void AddFieldsToInstantiate(
            Type target,
            MemberSerialization memberSerialization,
            JsonObjectAttribute jsonObjectAttribute,
            Dictionary<string, SerializedProperty> membersToInstantiate)
        {
            foreach (FieldInfo field in target.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                JsonPropertyAttribute jsonPropertyAttribute = field.GetCustomAttribute<JsonPropertyAttribute>();
                DataMemberAttribute dataMemberAttribute = field.GetCustomAttribute<DataMemberAttribute>();

                switch (memberSerialization)
                {
                    case MemberSerialization.Fields:
                        membersToInstantiate[field.Name] = CreateSerializedPropertyObject(field, jsonPropertyAttribute, dataMemberAttribute);
                        break;

                    case MemberSerialization.OptOut:
                        if (field.IsPublic
                            && field.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                        {
                            membersToInstantiate[field.Name] = CreateSerializedPropertyObject(field, jsonPropertyAttribute, dataMemberAttribute);
                        }
                        break;

                    case MemberSerialization.OptIn:
                        if (jsonPropertyAttribute != null
                            || dataMemberAttribute != null)
                        {
                            membersToInstantiate[field.Name] = CreateSerializedPropertyObject(field, jsonPropertyAttribute, dataMemberAttribute);
                        }
                        break;
                }
            }
        }

        private void AddPropertiesToInstantiate(
            Type target,
            MemberSerialization memberSerialization,
            JsonObjectAttribute jsonObjectAttribute,
            Dictionary<string, SerializedProperty> membersToInstantiate)
        {

            foreach (PropertyInfo property in target.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                JsonPropertyAttribute jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                DataMemberAttribute dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();

                switch (memberSerialization)
                {
                    case MemberSerialization.Fields:
                    case MemberSerialization.OptOut:
                        if (property.GetGetMethod().IsPublic
                            && property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                        {
                            membersToInstantiate[property.Name] = CreateSerializedPropertyObject(property, jsonPropertyAttribute, dataMemberAttribute);
                        }
                        break;

                    case MemberSerialization.OptIn:
                        if (jsonPropertyAttribute != null
                            || dataMemberAttribute != null)
                        {
                            membersToInstantiate[property.Name] = CreateSerializedPropertyObject(property, jsonPropertyAttribute, dataMemberAttribute);
                        }
                        break;
                }
            }
        }

        private SerializedProperty CreateSerializedPropertyObject(MemberInfo member, JsonPropertyAttribute jsonPropertyAttribute, DataMemberAttribute dataMemberAttribute)
        {
            return new SerializedProperty
            {
                MemberInfo = member,
                JsonPropertyAttribute = jsonPropertyAttribute,
                DataMemberAttribute = dataMemberAttribute,
                Name = jsonPropertyAttribute?.PropertyName ?? dataMemberAttribute?.Name ?? member.Name,
            };
        }

        private object InstantiateObject(
            Type target,
            ConstructorInfo ctor,
            Dictionary<string, ExpressionAst> hashtableDict,
            Dictionary<string, SerializedProperty> membersToInstantiate)
        {
            var ctorParameters = new List<object>();
            foreach (ParameterInfo ctorParameter in ctor.GetParameters())
            {
                if (!membersToInstantiate.TryGetValue(ctorParameter.Name, out SerializedProperty serializedProperty))
                {
                    throw new ArgumentException($"The constructor for type '{target.FullName}' requires parameter '{ctorParameter.Name}' but has no such member");
                }

                ctorParameters.Add(InstantiateHashtablePropertyAsMember(
                    serializedProperty.MemberInfo,
                    hashtableDict,
                    serializedProperty.Name));

                hashtableDict.Remove(serializedProperty.Name);
            }

            return ctor.Invoke(ctorParameters.ToArray());
        } 

        private void SetObjectProperties(
            object instance,
            Dictionary<string, ExpressionAst> hashtableDict,
            IReadOnlyDictionary<string, SerializedProperty> membersToInstantiate)
        {
            foreach (SerializedProperty memberToInstantiate in membersToInstantiate.Values)
            {
                if (!hashtableDict.TryGetValue(memberToInstantiate.Name, out ExpressionAst expressionAst))
                {
                    continue;
                }

                hashtableDict.Remove(memberToInstantiate.Name);

                switch (memberToInstantiate.MemberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        var fieldInfo = (FieldInfo)memberToInstantiate.MemberInfo;

                        if (fieldInfo.IsInitOnly)
                        {
                            throw new ArgumentException($"Field '{fieldInfo.Name}' on type '{fieldInfo.DeclaringType.FullName}' is readonly and cannot be set");
                        }

                        fieldInfo.SetValue(instance, Convert(fieldInfo.FieldType, expressionAst));
                        break;

                    case MemberTypes.Property:
                        var propertyInfo = (PropertyInfo)memberToInstantiate.MemberInfo;
                        MethodInfo propertySetter = propertyInfo.GetSetMethod() ?? propertyInfo.GetSetMethod(nonPublic: true);

                        if (propertySetter == null)
                        {
                            throw new ArgumentException($"Property '{propertyInfo.Name}' on type '{propertyInfo.DeclaringType.FullName}' has no setter and cannot be set");
                        }

                        propertySetter.Invoke(instance, new[] { Convert(propertyInfo.PropertyType, expressionAst) });
                        break;
                }
            }

            if (hashtableDict.Count > 0)
            {
                throw new ArgumentException($"Unknown key(s) in hashtable: {string.Join(",", hashtableDict.Keys)}");
            }
        }

        private object ConvertPoco(Type target, ExpressionAst ast)
        {
            // Validate hashtable and convert to a key value dict
            Dictionary<string, ExpressionAst> hashtableDict = GetHashtableDict(target, ast);

            // Get the designated constructor
            ConstructorInfo designatedConstructor = GetDesignatedConstructor(target);

            // Get the properties we need to instantiate
            Dictionary<string, SerializedProperty> membersToInstantiate = GetMembersToInstantiate(target);

            // Instantiate object
            object instance = InstantiateObject(target, designatedConstructor, hashtableDict, membersToInstantiate);

            // Set the properties
            SetObjectProperties(instance, hashtableDict, membersToInstantiate);

            // return
            return instance;
        }

        private object InstantiateHashtablePropertyAsMember(MemberInfo memberInfo, IReadOnlyDictionary<string, ExpressionAst> hashtable, string name)
        {
            if (!hashtable.TryGetValue(name, out ExpressionAst expressionAst))
            {
                throw new ArgumentException($"Hashtable has no property '{name}'");
            }

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return Convert(((FieldInfo)memberInfo).FieldType, expressionAst);

                case MemberTypes.Property:
                    return Convert(((PropertyInfo)memberInfo).PropertyType, expressionAst);

                default:
                    throw new ArgumentException($"Bad member type '{memberInfo.MemberType}' on member {memberInfo.Name}");
            }
        }

        private bool TryConvertDictionary(Type target, ExpressionAst ast, out object result)
        {
            var hashtableAst = ast as HashtableAst;

            // In case a partial AST is requested
            if (target == typeof(HashtableAst))
            {
                if (hashtableAst == null)
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                result = hashtableAst;
                return true;
            }

            // If the types are specified as well known ones,
            // just instantiate them now
            if (target == typeof(Hashtable)
                || target == typeof(IDictionary))
            {
                if (hashtableAst == null)
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                var hashtable = new Hashtable();
                SetHashtableValuesInDictionary(hashtable, hashtableAst);
                result = hashtable;
                return true;
            }

            // If there's an unknown type that implements IDictionary,
            // look for a convenient constructor to instantiate it
            if (!target.IsGenericType)
            {
                if (!typeof(IDictionary).IsAssignableFrom(target))
                {
                    result = null;
                    return false;
                }

                if (hashtableAst == null)
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                ConstructorInfo defaultCtor = target.GetConstructor(new Type[0]);

                if (defaultCtor != null)
                {
                    var dict = (IDictionary)defaultCtor.Invoke(null);
                    SetHashtableValuesInDictionary(dict, hashtableAst);
                    result = dict;
                    return true;
                }

                ConstructorInfo injectionCtor = target.GetConstructor(new Type[] { typeof(IDictionary) })
                    ?? target.GetConstructor(new Type[] { typeof(Hashtable) });

                if (injectionCtor != null)
                {
                    var dict = new Hashtable();
                    SetHashtableValuesInDictionary(dict, hashtableAst);
                    result = injectionCtor.Invoke(new object[] { dict });
                    return true;
                }

                throw new ArgumentException($"Unable to instantiate type '{target}'");
            }

            // We have a generic type
            Type targetBaseGenericType = target.GetGenericTypeDefinition();
            Type[] genericTypeParameters = target.GetGenericArguments();

            // Guess that this is a normal dictionary type
            if (targetBaseGenericType == typeof(Dictionary<,>)
                || targetBaseGenericType == typeof(IDictionary<,>)
                || targetBaseGenericType == typeof(IReadOnlyDictionary<,>))
            {
                if (hashtableAst == null)
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                Type dictionaryType = typeof(Dictionary<,>)
                    .MakeGenericType(genericTypeParameters);

                object dict = dictionaryType
                    .GetConstructor(new Type[0])
                    .Invoke(null);

                SetHashtableValuesInDictionary(dictionaryType, genericTypeParameters[0], genericTypeParameters[1], dict, hashtableAst);
                result = dict;
                return true;
            }

            // Otherwise, it might implement a dictionary and have a default constructor 
            if (ImplementsGenericInterface(target, typeof(IDictionary<,>), out IReadOnlyList<Type> _))
            {
                if (hashtableAst == null)
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                ConstructorInfo defaultCtor = target.GetConstructor(new Type[0]);

                if (defaultCtor != null)
                {
                    var dict = defaultCtor.Invoke(null);
                    SetHashtableValuesInDictionary(target, genericTypeParameters[0], genericTypeParameters[1], dict, hashtableAst);
                    result = dict;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private void SetHashtableValuesInDictionary(IDictionary dictionary, HashtableAst hashtableAst)
        {
            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                object key = _psdDataParser.ConvertAstValue(entry.Item1);
                object value = _psdDataParser.ConvertAstValue(GetExpressionFromStatementAst(entry.Item2));
                dictionary[key] = value;
            }
        }

        private void SetHashtableValuesInDictionary(Type dictionaryType, Type keyType, Type valueType, object dictionary, HashtableAst hashtableAst)
        {
            MethodInfo setter = dictionaryType.GetProperty("Item").GetSetMethod();

            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                object key = Convert(keyType, entry.Item1);
                object value = Convert(valueType, GetExpressionFromStatementAst(entry.Item2));
                setter.Invoke(dictionary, new object[] { key, value });
            }
        }


        private bool TryConvertEnumerable(Type target, ExpressionAst ast, out object result)
        {
            if (target.IsArray)
            {
                if (!TryExtractExpressionArray(ast, out IReadOnlyList<ExpressionAst> arrayExpressions))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                Type elementType = target.GetElementType();

                Array array = Array.CreateInstance(elementType, arrayExpressions.Count);

                for (int i = 0; i < arrayExpressions.Count; i++)
                {
                    array.SetValue(Convert(elementType, arrayExpressions[i]), i);
                }

                result = array;
                return true;
            }

            if (target == typeof(IList)
                || target == typeof(IEnumerable))
            {
                if (!TryExtractExpressionArray(ast, out IReadOnlyList<ExpressionAst> arrayExpressions))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                var list = new ArrayList();
                AddItemsToList(list, arrayExpressions);

                result = list;
                return true;
            }

            if (typeof(IList).IsAssignableFrom(target))
            {
                if (!TryExtractExpressionArray(ast, out IReadOnlyList<ExpressionAst> arrayExpressions))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                ConstructorInfo defaultCtor = target.GetConstructor(new Type[0]);

                if (defaultCtor != null)
                {
                    var list = (IList)defaultCtor.Invoke(null);
                    AddItemsToList(list, arrayExpressions);
                    result = list;
                    return true;
                }

                throw new ArgumentException($"Unable to instantiate type '{target.FullName}'");
            }

            if (!target.IsGenericType)
            {
                result = null;
                return false;
            }

            // We have a generic type
            Type targetBaseGenericType = target.GetGenericTypeDefinition();
            Type[] genericTypeParameters = target.GetGenericArguments();

            if (targetBaseGenericType == typeof(IEnumerable<>)
                || targetBaseGenericType == typeof(List<>)
                || targetBaseGenericType == typeof(IList<>)
                || targetBaseGenericType == typeof(IReadOnlyList<>)
                || targetBaseGenericType == typeof(IReadOnlyCollection<>))
            {
                if (!TryExtractExpressionArray(ast, out IReadOnlyList<ExpressionAst> arrayExpressions))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                var list = (IList)typeof(List<>)
                    .MakeGenericType(genericTypeParameters)
                    .GetConstructor(new Type[0])
                    .Invoke(null);
                AddItemsToList(genericTypeParameters[0], list, arrayExpressions);
                result = list;
                return true;
            }

            if (ImplementsGenericInterface(target, typeof(IList<>), out IReadOnlyList<Type> _))
            {
                if (!TryExtractExpressionArray(ast, out IReadOnlyList<ExpressionAst> arrayExpressions))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                ConstructorInfo defaultConstructor = target.GetConstructor(new Type[0]);
                if (defaultConstructor != null)
                {
                    object list = defaultConstructor.Invoke(null);
                    AddItemsToList(target, genericTypeParameters[0], list, arrayExpressions);
                    result = list;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private void AddItemsToList(IList list, IEnumerable<ExpressionAst> expressions)
        {
            foreach (ExpressionAst expression in expressions)
            {
                list.Add(Convert(typeof(object), expression));
            }
        }

        private void AddItemsToList(Type elementType, IList list, IEnumerable<ExpressionAst> expressions)
        {
            foreach (ExpressionAst expression in expressions)
            {
                list.Add(Convert(elementType, expression));
            }
        }

        private void AddItemsToList(Type targetType, Type elementType, object list, IEnumerable<ExpressionAst> expressions)
        {
            MethodInfo setter = targetType.GetProperty("Item").GetSetMethod();

            foreach (ExpressionAst expression in expressions)
            {
                setter.Invoke(list, new object[] { Convert(elementType, expression) });
            }
        }

        private bool TryConvertVariableExpression(Type target, ExpressionAst ast, out object result)
        {
            if (!(ast is VariableExpressionAst variableExpressionAst))
            {
                result = null;
                return false;
            }

            string variableName = variableExpressionAst.VariablePath.UserPath;

            if (variableName.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                if (target.IsValueType
                    && !(target.IsGenericType && target.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    throw new ArgumentException($"Cannot convert value type '{target.FullName}' to null");
                }

                result = null;
                return true;
            }

            if (target == typeof(bool))
            {
                if (variableName.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    result = true;
                    return true;
                }

                if (variableName.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    result = false;
                    return true;
                }

                throw new ArgumentException($"Cannot convert '{ast}' to boolean type");
            }

            throw new ArgumentException($"Cannot convert non-constant variable '{ast}' to value");
        }

        private string ConvertString(ExpressionAst ast)
        {
            if (ast is StringConstantExpressionAst stringConstantExpression)
            {
                return stringConstantExpression.Value;
            }

            throw CreateConversionMismatchException(ast, typeof(string));
        }

        private bool TryConvertNumber(Type target, ExpressionAst ast, out object result)
        {
            if (IsNumericType(target))
            {
                if (!(ast is ConstantExpressionAst constantExpression)
                    || !IsNumericType(constantExpression.StaticType))
                {
                    throw CreateConversionMismatchException(ast, target);
                }

                result = System.Convert.ChangeType(constantExpression.Value, target);
                return true;
            }

            result = null;
            return false;
        }

        private DateTime ConvertDateTime(ExpressionAst ast)
        {
            if (ast is StringConstantExpressionAst stringConstantExpressionAst
                && DateTime.TryParse(stringConstantExpressionAst.Value, out DateTime dateTime))
            {
                return dateTime;
            }

            throw CreateConversionMismatchException(ast, typeof(DateTime));
        }

        private bool TryExtractExpressionArray(ExpressionAst ast, out IReadOnlyList<ExpressionAst> arrayItems)
        {
            switch (ast)
            {
                case ArrayExpressionAst arrayExpressionAst:
                    var expressions = new List<ExpressionAst>();

                    foreach (StatementAst statementAst in arrayExpressionAst.SubExpression.Statements)
                    {
                        ExpressionAst expression = GetExpressionFromStatementAst(statementAst);

                        switch (expression)
                        {
                            case ArrayLiteralAst subArrayLiteral:
                                expressions.AddRange(subArrayLiteral.Elements);
                                break;

                            default:
                                expressions.Add(expression);
                                break;
                        }
                    }

                    arrayItems = expressions;
                    return true;

                case ArrayLiteralAst arrayLiteralAst:
                    arrayItems = arrayLiteralAst.Elements;
                    return true;

                default:
                    arrayItems = null;
                    return false;
            }
        }

        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return true;
            }

            return false;
        }

        private ExpressionAst GetExpressionFromStatementAst(StatementAst stmtAst)
        {
            return (stmtAst as PipelineAst)?.GetPureExpression()
                ?? throw new ArgumentException($"'{stmtAst}' must be a pure pipeline AST to convert a value");
        }

        private static Exception CreateConversionMismatchException(Ast ast, Type type)
        {
            return new ArgumentException($"Unable to convert ast '{ast.Extent.Text}' of type '{ast.GetType().FullName}' to type '{type.FullName}'");
        }

        private static bool ImplementsGenericInterface(
            Type target,
            Type genericInterface,
            out IReadOnlyList<Type> genericParameters)
        {
            foreach (Type implementedInterface in target.GetInterfaces())
            {
                if (!implementedInterface.IsGenericType)
                {
                    continue;
                }

                if (implementedInterface.GetGenericTypeDefinition() == genericInterface)
                {
                    genericParameters = implementedInterface.GetGenericArguments();
                    return true;
                }
            }

            genericParameters = null;
            return false;
        }

        private class SerializedProperty
        {
            public MemberInfo MemberInfo { get; set; }

            public JsonPropertyAttribute JsonPropertyAttribute { get; set; }

            public DataMemberAttribute DataMemberAttribute { get; set; }

            public string Name { get; set; }
        }
    }
}
