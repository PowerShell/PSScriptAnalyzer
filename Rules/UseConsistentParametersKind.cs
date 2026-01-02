// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseConsistentParametersKind: Checks if function parameters definition kind is same as preferred.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConsistentParametersKind : ConfigurableRule
    {
        private enum ParametersDefinitionKind
        {
            Inline,
            ParamBlock
        }

        private ParametersDefinitionKind parametersKind;

        /// <summary>
        /// Construct an object of UseConsistentParametersKind type.
        /// </summary>
        public UseConsistentParametersKind() : base()
        {
            Enable = false;  // Disable rule by default
        }

        /// <summary>
        /// The type of preferred parameters definition for functions.
        ///
        /// Default value is "ParamBlock".
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: "ParamBlock")]
        public string ParametersKind
        {
            get
            {
                return parametersKind.ToString();
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value) ||
                    !Enum.TryParse<ParametersDefinitionKind>(value, true, out parametersKind))
                {
                    parametersKind = ParametersDefinitionKind.ParamBlock;
                }
            }
        }

        /// <summary>
        /// AnalyzeScript: Analyze the script to check if any function is using not preferred parameters kind.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) { throw new ArgumentNullException(Strings.NullAstErrorMessage); }

            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            if (parametersKind == ParametersDefinitionKind.ParamBlock)
            {
                return checkInlineParameters(functionAsts, fileName);
            }
            else
            {
                return checkParamBlockParameters(functionAsts, fileName);
            }
        }

        private IEnumerable<DiagnosticRecord> checkInlineParameters(IEnumerable<Ast> functionAsts, string fileName)
        {
            foreach (FunctionDefinitionAst functionAst in functionAsts)
            {
                if (functionAst.Parameters != null)
                {
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentParametersKindInlineError, functionAst.Name),
                        functionAst.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    );
                }
            }
        }

        private IEnumerable<DiagnosticRecord> checkParamBlockParameters(IEnumerable<Ast> functionAsts, string fileName)
        {
            foreach (FunctionDefinitionAst functionAst in functionAsts)
            {
                if (functionAst.Body.ParamBlock != null)
                {
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentParametersKindParamBlockError, functionAst.Name),
                        functionAst.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    );
                }
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentParametersKindCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentParametersKindDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseConsistentParametersKindName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
