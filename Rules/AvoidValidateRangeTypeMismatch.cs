// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidValidateRangeTypeMismatch: Detect integer ValidateRange bounds on floating-point and decimal parameters.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public sealed class AvoidValidateRangeTypeMismatch : IScriptRule
    {
        private static readonly HashSet<Type> FloatingPointAndDecimalTypes = new HashSet<Type>
        {
            typeof(double),
            typeof(float),
            typeof(decimal)
        };

        private static readonly HashSet<Type> IntegralTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast is null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            IEnumerable<Ast> parameterAsts = ast.FindAll(testAst => testAst is ParameterAst, searchNestedScriptBlocks: true);
            foreach (ParameterAst parameterAst in parameterAsts)
            {
                TypeConstraintAst typeConstraint = parameterAst.Attributes.OfType<TypeConstraintAst>().FirstOrDefault();
                if (typeConstraint == null)
                {
                    continue;
                }

                Type parameterType = typeConstraint.TypeName.GetReflectionType();
                if (parameterType == null)
                {
                    continue;
                }

                if (parameterType.IsArray)
                {
                    parameterType = parameterType.GetElementType();
                }

                parameterType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

                if (!FloatingPointAndDecimalTypes.Contains(parameterType))
                {
                    continue;
                }

                foreach (AttributeAst attributeAst in parameterAst.Attributes.OfType<AttributeAst>())
                {
                    if (!IsValidateRangeAttribute(attributeAst)
                        || attributeAst.PositionalArguments.Count < 2
                        || !attributeAst.PositionalArguments.Take(2).All(IsIntegralBoundary))
                    {
                        continue;
                    }

                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidValidateRangeTypeMismatchError, parameterAst.Name, typeConstraint.TypeName.FullName),
                        attributeAst.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName);
                }
            }
        }

        private static bool IsIntegralBoundary(ExpressionAst expressionAst)
        {
            if (expressionAst is ConstantExpressionAst constantExpressionAst)
            {
                return constantExpressionAst.Value != null && IntegralTypes.Contains(constantExpressionAst.Value.GetType());
            }

            if (expressionAst is ConvertExpressionAst convertExpressionAst)
            {
                return IntegralTypes.Contains(convertExpressionAst.Type.TypeName.GetReflectionType());
            }

            if (expressionAst is UnaryExpressionAst unaryExpressionAst
                && (unaryExpressionAst.TokenKind == TokenKind.Minus || unaryExpressionAst.TokenKind == TokenKind.Plus))
            {
                return IsIntegralBoundary(unaryExpressionAst.Child);
            }

            if (expressionAst is MemberExpressionAst memberExpressionAst
                && memberExpressionAst.Static
                && memberExpressionAst.Expression is TypeExpressionAst typeExpressionAst
                && memberExpressionAst.Member is StringConstantExpressionAst memberName)
            {
                Type type = typeExpressionAst.TypeName.GetReflectionType();
                FieldInfo field = type?.GetField(memberName.Value, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy);
                return field != null && field.IsLiteral && IntegralTypes.Contains(field.FieldType);
            }

            return false;
        }

        private static bool IsValidateRangeAttribute(AttributeAst attributeAst)
        {
            return attributeAst.TypeName.GetReflectionAttributeType() == typeof(System.Management.Automation.ValidateRangeAttribute);
        }

        public string GetName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidValidateRangeTypeMismatchName);
        }

        public string GetCommonName()
        {
            return Strings.AvoidValidateRangeTypeMismatchCommonName;
        }

        public string GetDescription()
        {
            return Strings.AvoidValidateRangeTypeMismatchDescription;
        }

        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public string GetSourceName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
