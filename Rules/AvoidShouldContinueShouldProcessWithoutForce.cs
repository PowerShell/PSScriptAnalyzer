using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidShouldContinueWithoutForceParameter: Check that if ShouldContinue is used,
    /// the function should have a boolean force parameter
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidShouldContinueShouldProcessWithoutForce : IScriptRule
    {
        /// <summary>
        /// AvoidShouldContinueWithoutForceParameter: Check that if ShouldContinue is used,
        /// the function should have a boolean force parameter
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all ParamAsts.
            IEnumerable<Ast> funcAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            // Iterrates all ParamAsts and check if there are any force.
            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                IEnumerable<Ast> paramAsts = funcAst.FindAll(testAst => testAst is ParameterAst, true);
                bool hasForce = false;

                foreach (ParameterAst paramAst in paramAsts)
                {
                    if (String.Equals(paramAst.Name.VariablePath.ToString(), "force", StringComparison.OrdinalIgnoreCase)
                        && String.Equals(paramAst.StaticType.FullName, "System.Boolean", StringComparison.OrdinalIgnoreCase))
                    {
                        hasForce = true;
                        break;
                    }
                }

                if (hasForce)
                {
                    continue;
                }

                IEnumerable<Ast> imeAsts = funcAst.FindAll(testAst => testAst is InvokeMemberExpressionAst, true);

                foreach (InvokeMemberExpressionAst imeAst in imeAsts)
                {
                    VariableExpressionAst typeAst = imeAst.Expression as VariableExpressionAst;
                    if (typeAst == null) continue;

                    if (String.Equals(typeAst.VariablePath.UserPath, "pscmdlet", StringComparison.OrdinalIgnoreCase)
                        && (String.Equals(imeAst.Member.Extent.Text, "shouldcontinue", StringComparison.OrdinalIgnoreCase)
                            || String.Equals(imeAst.Member.Extent.Text, "shouldprocess", StringComparison.OrdinalIgnoreCase)))
                    {
                        yield return new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueShouldProcessWithoutForceError, funcAst.Name, System.IO.Path.GetFileName(fileName)),
                            imeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidShouldContinueShouldProcessWithoutForceName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueShouldProcessWithoutForceCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueShouldProcessWithoutForceDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
