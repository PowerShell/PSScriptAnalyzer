// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
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
        private static readonly HashSet<string> FloatingPointAndDecimalTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "double", "System.Double",
            "single", "float", "System.Single",
            "decimal", "System.Decimal"
        };

        private static readonly HashSet<string> IntegralTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "byte", "System.Byte",
            "sbyte", "System.SByte",
            "short", "System.Int16",
            "ushort", "System.UInt16",
            "int", "System.Int32",
            "uint", "System.UInt32",
            "long", "System.Int64",
            "ulong", "System.UInt64"
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
                if (typeConstraint == null || !FloatingPointAndDecimalTypes.Contains(typeConstraint.TypeName.FullName))
                {
                    continue;
                }

                foreach (AttributeAst attributeAst in parameterAst.Attributes.OfType<AttributeAst>())
                {
                    if (!IsValidateRangeAttribute(attributeAst)
                        || attributeAst.PositionalArguments.Count < 2
                        || !attributeAst.PositionalArguments.Take(2).Any(IsIntegralBoundary))
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
                return constantExpressionAst.Value != null
                    && IntegralTypes.Contains(constantExpressionAst.Value.GetType().FullName);
            }

            if (expressionAst is ConvertExpressionAst convertExpressionAst)
            {
                return IntegralTypes.Contains(convertExpressionAst.Type.TypeName.FullName)
                    && IsIntegralBoundary(convertExpressionAst.Child);
            }

            if (expressionAst is UnaryExpressionAst unaryExpressionAst
                && (unaryExpressionAst.TokenKind == TokenKind.Minus || unaryExpressionAst.TokenKind == TokenKind.Plus))
            {
                return IsIntegralBoundary(unaryExpressionAst.Child);
            }

            return false;
        }

        private static bool IsValidateRangeAttribute(AttributeAst attributeAst)
        {
            string attributeName = attributeAst.TypeName.FullName;
            int lastDot = attributeName.LastIndexOf('.');
            string simpleName = lastDot < 0 ? attributeName : attributeName.Substring(lastDot + 1);
            return String.Equals(simpleName, "ValidateRange", StringComparison.OrdinalIgnoreCase)
                || String.Equals(simpleName, "ValidateRangeAttribute", StringComparison.OrdinalIgnoreCase);
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
