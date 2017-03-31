// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// A class to that implements the UseSupportsShouldProcess rule.
    /// </summary>
    #if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class UseSupportsShouldProcess : ConfigurableRule
    {
        /// <summary>
        /// Analyzes the given ast to find if a function defines Confirm and/or WhatIf parameters manually
        /// instead of using SupportShouldProcess attribute.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            // your code goes here
            return FindViolations(ast);
        }

        private List<DiagnosticRecord> FindViolations(Ast ast)
        {
            var foundAsts = ast.FindAll(x => x is FunctionDefinitionAst, true);
            var diagnosticRecords = new List<DiagnosticRecord>();
            foreach (var foundAst in foundAsts)
            {
                var functionDefinitionAst = foundAst as FunctionDefinitionAst;
                if (AddsWhatIf(functionDefinitionAst) || AddsConfirm(functionDefinitionAst))
                {
                    diagnosticRecords.Add(new DiagnosticRecord());
                }
            }

            return diagnosticRecords;
        }

        private bool AddsConfirm(FunctionDefinitionAst functionDefinitionAst)
        {
            return AddsParameter(functionDefinitionAst, "confirm");
        }

        private bool AddsWhatIf(FunctionDefinitionAst functionDefinitionAst)
        {
            return AddsParameter(functionDefinitionAst, "whatif");
        }

        private bool AddsParameter(FunctionDefinitionAst functionDefinitionAst, string parameter)
        {
            if (functionDefinitionAst.Parameters != null)
            {
                return ParametersContain(functionDefinitionAst.Parameters, parameter);
            }
            else if (functionDefinitionAst.Body.ParamBlock?.Parameters != null)
            {
                return ParametersContain(functionDefinitionAst.Body.ParamBlock.Parameters, parameter);
            }

            return false;
        }

        private bool ParametersContain(ICollection<ParameterAst> parameterAsts, string parameter)
        {
            foreach (var paramAst in parameterAsts)
            {
                if(paramAst.Name.VariablePath.UserPath.Equals(parameter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSupportsShouldProcessCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSupportsShouldProcessDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseSupportsShouldProcessName);
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
    }
}
