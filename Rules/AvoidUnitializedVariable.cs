using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using Microsoft.Windows.Powershell.ScriptAnalyzer;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUnitializedVariable: Check if any uninitialized variable is used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUnitializedVariable : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if any uninitialized variable is used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            // TODO : We need to find another way for using S.M.A.L.Compiler.GetExpressionValue.
            // Following code are not working for certain scenarios;, like:
            // for ($i=1; $i -le 10; $i++){Write-Host $i}

            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all VariableExpressionAst
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is VariableExpressionAst, true);

            // Iterates all VariableExpressionAst and check the command name.
            foreach (VariableExpressionAst varAst in foundAsts)
            {
                if (Helper.Instance.IsUninitialized(varAst, ast))
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableError, varAst.VariablePath.UserPath),
                        varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName, varAst.VariablePath.UserPath);
                }
            }

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst || item is FunctionMemberAst, true);

            foreach (var funcAst in funcAsts)
            {
                // Finds all VariableExpressionAst.
                IEnumerable<Ast> varAsts = funcAst.FindAll(testAst => testAst is VariableExpressionAst, true);

                // Iterates all VariableExpressionAst and check the command name.
                foreach (VariableExpressionAst varAst in varAsts)
                {
                    if (Helper.Instance.IsUninitialized(varAst, funcAst))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableError, varAst.VariablePath.UserPath),
                            varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName, varAst.VariablePath.UserPath);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUninitializedVariableName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
