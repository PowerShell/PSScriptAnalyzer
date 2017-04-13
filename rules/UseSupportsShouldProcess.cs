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
                int whatIfIndex, confirmIndex;
                ParameterAst whatIfParamAst, confirmParamAst;
                ParamBlockAst paramBlockAst;
                ParameterAst[] parameterAsts = GetParameters(functionDefinitionAst, out paramBlockAst);
                if (parameterAsts == null)
                {
                    continue;
                }


                // get paramterAsts and parameter block
                // then use get parameter on the parameterAsts
                var addsWhatIf = TryGetParameterAst(parameterAsts, "whatif", out whatIfIndex);
                whatIfParamAst = addsWhatIf ? parameterAsts[whatIfIndex] : null;
                var addsConfirm = TryGetParameterAst(parameterAsts, "confirm", out confirmIndex);
                confirmParamAst = addsConfirm ? parameterAsts[confirmIndex] : null;

                if (addsWhatIf || addsConfirm)
                {
                    IScriptExtent scriptExtent;
                    if (addsWhatIf && addsConfirm)
                    {
                        // mark everthing between the parameters including them
                        scriptExtent = CombineExtents(whatIfParamAst.Extent, confirmParamAst.Extent);
                    }
                    else if (addsWhatIf)
                    {
                        // mark the whatif parameter
                        scriptExtent = whatIfParamAst.Extent;
                    }
                    else
                    {
                        // mark the confirm parameter
                        scriptExtent = confirmParamAst.Extent;
                    }

                    diagnosticRecords.Add(new DiagnosticRecord(
                        GetError(functionDefinitionAst.Name),
                        scriptExtent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        scriptExtent.File,
                        null,
                        GetCorrections(whatIfIndex, confirmIndex, parameterAsts, paramBlockAst, functionDefinitionAst)));
                }
            }

            return diagnosticRecords;
        }

        private static ParameterAst[] GetParameters(
            FunctionDefinitionAst functionDefinitionAst,
            out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = null;
            if (functionDefinitionAst.Parameters != null)
            {
                return new List<ParameterAst>(functionDefinitionAst.Parameters).ToArray();
            }
            else if (functionDefinitionAst.Body.ParamBlock?.Parameters != null)
            {
                paramBlockAst = functionDefinitionAst.Body.ParamBlock;
                return new List<ParameterAst>(functionDefinitionAst.Body.ParamBlock.Parameters).ToArray();
            }

            return null;
        }

        private List<CorrectionExtent> GetCorrections(
            int whatIfIndex,
            int confirmIndex,
            ParameterAst[] parameterAsts,
            ParamBlockAst paramBlockAst,
            FunctionDefinitionAst funcDefnAst)
        {
            // we need to handle the following cases
            // - If a function has paramBlockAst
            //  - if it has cmdletbinding
            //    - if it has supportsshouldprocess
            //    - else it doesn't have supportsshouldprocess
            //  - else it does not have cmdlet binding
            // - else a function doesn't have paramBlockAst
            IScriptExtent whatIfExtent, confirmExtent;
            var filePath = funcDefnAst.Extent.File;
            var correctionExtents = new List<CorrectionExtent>();

            // replace whatif/confirm extent starting with the first character ending with the first character of
            // the next parameter
            if (whatIfIndex != -1)
            {
                correctionExtents.Add(GetCorrectionExtent(whatIfIndex, parameterAsts));
            }

            if (confirmIndex != -1)
            {
                correctionExtents.Add(GetCorrectionExtent(confirmIndex, parameterAsts));
            }

            if (paramBlockAst != null)
            {

            }
            else
            {
                // function doesn't have param block
                // replace
            }

            // This is how we handle multiple edits.
            // create separate edits
            // apply those edits to the original script extent1
            // and then give the corrected extent as suggested correction.
            return correctionExtents;
        }

        private static CorrectionExtent GetCorrectionExtent(
            int paramIndex,
            ParameterAst[] parameterAsts)
        {
            IScriptExtent paramExtent = parameterAsts[paramIndex].Extent;
            int endLineNumber, endColumnNumber;
            if (paramIndex < parameterAsts.Length - 1)
            {
                endLineNumber = parameterAsts[paramIndex + 1].Extent.StartLineNumber;
                endColumnNumber = parameterAsts[paramIndex + 1].Extent.StartColumnNumber;
            }
            else
            {
                endLineNumber = paramExtent.EndLineNumber;
                endColumnNumber = paramExtent.EndColumnNumber;
            }

            return new CorrectionExtent(
                paramExtent.StartLineNumber,
                endLineNumber,
                paramExtent.StartColumnNumber,
                endColumnNumber,
                "",
                paramExtent.File);
        }

        private IScriptExtent CombineExtents(IScriptExtent extent1, IScriptExtent extent2)
        {
            IScriptExtent sExt, eExt;

            // There are many conditions that we need to consider but for now we are considering
            // this special case only.
            if (extent1.StartOffset < extent2.StartOffset)
            {
                sExt = extent1;
                eExt = extent2;
            }
            else
            {
                sExt = extent2;
                eExt = extent1;
            }

            return new ScriptExtent(
                new ScriptPosition(sExt.File, sExt.StartLineNumber, sExt.StartColumnNumber, null),
                new ScriptPosition(eExt.File, eExt.EndLineNumber, eExt.EndColumnNumber, null));
        }

        private bool TryGetParameterAst(
            FunctionDefinitionAst functionDefinitionAst,
            string parameter,
            out int parameterIndex,
            out ParameterAst[] parameters,
            out ParamBlockAst paramBlockAst)
        {
            if (functionDefinitionAst.Parameters != null)
            {
                if (TryGetParameterAst(functionDefinitionAst.Parameters, parameter, out parameterIndex))
                {
                    parameters = new List<ParameterAst>(functionDefinitionAst.Parameters).ToArray();
                    paramBlockAst = null;
                    functionDefinitionAst.Parameters.CopyTo(parameters, 0);
                    return true;
                }
            }
            else if (functionDefinitionAst.Body.ParamBlock?.Parameters != null)
            {
                if (TryGetParameterAst(
                    functionDefinitionAst.Body.ParamBlock.Parameters,
                    parameter,
                    out parameterIndex))
                {
                    parameters = new List<ParameterAst>(functionDefinitionAst.Body.ParamBlock.Parameters).ToArray();
                    paramBlockAst = functionDefinitionAst.Body.ParamBlock;
                    return true;
                }
            }

            parameterIndex = -1;
            paramBlockAst = null;
            parameters = null;
            return false;
        }

        private bool TryGetParameterAst(
            ICollection<ParameterAst> parameterAsts,
            string parameter,
            out int paramIndex)
        {
            paramIndex = 0;
            foreach (var paramAst in parameterAsts)
            {
                if (paramAst.Name.VariablePath.UserPath.Equals(parameter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                ++paramIndex;
            }

            paramIndex = -1;
            return false;
        }

        private string GetError(string functionName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseSupportsShouldProcessError,
                functionName);
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
