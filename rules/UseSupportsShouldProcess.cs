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
using System.Linq;
using System.Text;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;

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
        private const char whitespace = ' ';
        private const int indentationSize = 4;
        private Ast ast;
        private Token[] tokens;
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
            this.ast = ast;
            this.tokens = Helper.Instance.Tokens;
            return FindViolations();
        }

        private List<DiagnosticRecord> FindViolations()
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

                    // if both whatif and confirm are present,
                    // the extent will only contain whatif.
                    // we do this because editorservices relies on the text
                    // property of IScriptExtent and if we create a new extent
                    // with null Text value, it crashes editorservices.
                    if (addsWhatIf)
                    {
                        scriptExtent = whatIfParamAst.Extent;
                    }
                    else
                    {
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
            var filePath = funcDefnAst.Extent.File;
            var correctionExtents = new List<CorrectionExtent>();

            if (paramBlockAst != null)
            {
                if (whatIfIndex != -1)
                {
                    // TODO update method name to reflect its purpose.
                    correctionExtents.Add(GetCorrectionToRemoveParam(whatIfIndex, parameterAsts));
                }

                if (confirmIndex != -1)
                {
                    correctionExtents.Add(GetCorrectionToRemoveParam(confirmIndex, parameterAsts));
                }

                AttributeAst attributeAst;

                // check if it has cmdletbinding attribute
                if (TryGetCmdletBindingAttribute(paramBlockAst, out attributeAst))
                {
                    if (!attributeAst.NamedArguments.Any(
                        x => x.ArgumentName.Equals("supportsshouldprocess",
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        // add supportsshouldprocess to the attribute
                        correctionExtents.Add(GetCorrectionToAddShouldProcess(attributeAst));
                    }
                }
                else
                {
                    // has no cmdletbinding attribute
                    // hence, add the attribute and supportsshouldprocess argument
                    correctionExtents.Add(GetCorrectionToAddParamBlock(paramBlockAst));
                }
            }
            else
            {
                // function doesn't have param block
                // remove the parameter list
                // and create an equivalent param block
                // add cmdletbinding attribute and add supportsshouldprocess to it.
                correctionExtents.Add(GetCorrectionToRemoveFuncParamDecl(funcDefnAst, ast, tokens));
                correctionExtents.Add(GetCorrectionToAddParamBlock(funcDefnAst, parameterAsts));
            }

            // This is how we handle multiple edits-
            // create separate edits
            // apply those edits to the original script extent
            // and then give the corrected extent as suggested correction.

            // sort in descending order of start position
            correctionExtents.Sort((x, y) =>
            {
                var xRange = (Range)x;
                var yRange = (Range)y;
                return xRange.Start < yRange.Start ? 1 : (xRange.Start == yRange.Start ? 0 : -1);
            });

            var editableText = new EditableText(funcDefnAst.Extent.Text);
            foreach (var correctionExtent in correctionExtents)
            {
                var shiftedCorrectionExtent = Normalize(funcDefnAst.Extent, correctionExtent);
                editableText = editableText.ApplyEdit1(shiftedCorrectionExtent);
            }

            var result = new List<CorrectionExtent>();
            result.Add(
                new CorrectionExtent(
                funcDefnAst.Extent.StartLineNumber,
                funcDefnAst.Extent.EndLineNumber,
                funcDefnAst.Extent.StartColumnNumber,
                funcDefnAst.Extent.EndColumnNumber,
                editableText.ToString(),
                funcDefnAst.Extent.File));
            return result;
        }

        private CorrectionExtent GetCorrectionToAddParamBlock(
            FunctionDefinitionAst funcDefnAst,
            ParameterAst[] parameterAsts)
        {
            var funcStartScriptPos = funcDefnAst.Extent.StartScriptPosition;
            var paramBlockText = WriteParamBlock(parameterAsts);
            var indentation = new string(whitespace, funcStartScriptPos.ColumnNumber + 3);
            var lines = new List<String>();
            lines.Add("{");
            lines.AddRange(paramBlockText.Select(line => indentation + line));
            return new CorrectionExtent(
                funcDefnAst.Body.Extent.StartLineNumber,
                funcDefnAst.Body.Extent.StartLineNumber,
                funcDefnAst.Body.Extent.StartColumnNumber,
                funcDefnAst.Body.Extent.StartColumnNumber + 1,
                lines,
                funcDefnAst.Extent.File,
                null);
        }

        private IList<String> WriteParamBlock(ParameterAst[] parameterAsts)
        {
            var lines = new List<String>();
            lines.Add("[CmdletBinding(SupportsShouldProcess)]");
            lines.Add("param(");

            string indentation = new string(whitespace, indentationSize);
            int count = 0;
            var usableParamAsts = parameterAsts.Where(p => !IsWhatIf(p) && !IsConfirm(p));
            var usableCount = usableParamAsts.Count();
            foreach (var paramAst in usableParamAsts)
            {
                count++;
                var suffix = ",";
                if (count == usableCount)
                {
                    suffix = "";
                }

                foreach (var line in paramAst.Extent.Text.GetLines())
                {
                    lines.Add(indentation + line + suffix);
                }
            }

            lines.Add(")");
            return lines;
        }


        private static CorrectionExtent GetCorrectionToRemoveFuncParamDecl(
            FunctionDefinitionAst funcDefnAst,
            Ast ast,
            Token[] tokens)
        {
            var funcDefnTokens = TokenOperations.GetTokens(ast, funcDefnAst, tokens).ToArray();
            var lParenTokenIdx = Array.FindIndex(funcDefnTokens, tok => tok.Kind == TokenKind.LParen);
            var rParenTokenIdx = Array.FindIndex(funcDefnTokens, tok => tok.Kind == TokenKind.RParen);

            return new CorrectionExtent(
                funcDefnTokens[lParenTokenIdx - 1].Extent.EndLineNumber,
                funcDefnTokens[rParenTokenIdx].Extent.EndLineNumber,
                funcDefnTokens[lParenTokenIdx - 1].Extent.EndColumnNumber,
                funcDefnTokens[rParenTokenIdx].Extent.EndColumnNumber,
                "",
                ast.Extent.File);
        }

        // doesn't seem right. The arguments should be of same type.
        private CorrectionExtent Normalize(IScriptExtent referenceExtent, CorrectionExtent cextent)
        {
            // TODO Add ToRange extension methods for this conversion
            var refRange = new Range(
                referenceExtent.StartLineNumber,
                referenceExtent.StartColumnNumber,
                referenceExtent.EndLineNumber,
                referenceExtent.EndColumnNumber);

            var shiftedRange = Range.Normalize(refRange, cextent);

            // TODO Add a method to TextEdit class that takes in range and text
            // TODO Add a method to CorrectionExtent that takes in range and all other stuff
            return new CorrectionExtent(
                 shiftedRange.Start.Line,
                 shiftedRange.End.Line,
                 shiftedRange.Start.Column,
                 shiftedRange.End.Column,
                 cextent.Text,
                 cextent.File,
                 cextent.Description);
        }
        private static bool TryGetCmdletBindingAttribute(
            ParamBlockAst paramBlockAst,
            out AttributeAst attributeAst)
        {
            attributeAst = paramBlockAst.Attributes.FirstOrDefault(attr =>
            {
                return attr.TypeName.Name.Equals("cmdletbinding", StringComparison.OrdinalIgnoreCase);
            });

            return attributeAst != null;
        }

        private static CorrectionExtent GetCorrectionToAddParamBlock(ParamBlockAst paramBlockAst)
        {
            return new CorrectionExtent(
                paramBlockAst.Extent.StartLineNumber,
                paramBlockAst.Extent.StartLineNumber,
                1,
                1,
                new String[] {
                    new String(whitespace, paramBlockAst.Extent.StartColumnNumber - 1)
                        + "[CmdletBinding(SupportsShouldProcess)]",
                    String.Empty
                },
                null,
                null);
        }
        private static CorrectionExtent GetCorrectionToAddShouldProcess(AttributeAst cmdletBindingAttributeAst)
        {
            // 1 for the next position.
            var startColumnNumber = cmdletBindingAttributeAst.Extent.Text.IndexOf("(")
                + cmdletBindingAttributeAst.Extent.StartColumnNumber
                + 1;
            var extent = cmdletBindingAttributeAst.Extent;
            var suffix = cmdletBindingAttributeAst.NamedArguments.Count > 0
                || cmdletBindingAttributeAst.PositionalArguments.Count > 0
                    ? ", "
                    : "";
            return new CorrectionExtent(
                extent.StartLineNumber,
                extent.StartLineNumber,
                startColumnNumber,
                startColumnNumber,
                "SupportsShouldProcess" + suffix,
                extent.File);
        }

        private static CorrectionExtent GetCorrectionToRemoveParam(
            int paramIndex,
            ParameterAst[] parameterAsts)
        {
            IScriptExtent paramExtent = parameterAsts[paramIndex].Extent;
            int startLineNumber, startColumnNumber, endLineNumber, endColumnNumber;

            startLineNumber = paramExtent.StartLineNumber;
            startColumnNumber = paramExtent.StartColumnNumber;
            if (paramIndex < parameterAsts.Length - 1)
            {

                endLineNumber = parameterAsts[paramIndex + 1].Extent.StartLineNumber;
                endColumnNumber = parameterAsts[paramIndex + 1].Extent.StartColumnNumber;
            }
            else
            {
                // if last item in the parameter list then need to remove the
                // trailing comma after the previous parameter.
                if (paramIndex > 0)
                {
                    var lp = parameterAsts[paramIndex - 1];
                    if (!IsWhatIf(lp) && !IsConfirm(lp))
                    {
                        startLineNumber = lp.Extent.EndLineNumber;
                        startColumnNumber = lp.Extent.EndColumnNumber;
                    }
                }
                endLineNumber = paramExtent.EndLineNumber;
                endColumnNumber = paramExtent.EndColumnNumber;
            }

            return new CorrectionExtent(
                startLineNumber,
                endLineNumber,
                startColumnNumber,
                endColumnNumber,
                "",
                paramExtent.File);
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
                if (IsParameter(paramAst, parameter))
                {
                    return true;
                }

                ++paramIndex;
            }

            paramIndex = -1;
            return false;
        }

        private static bool IsParameter(ParameterAst parameterAst, string parameterName)
        {
            return parameterAst.Name.VariablePath.UserPath.Equals(
                parameterName,
                StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsWhatIf(ParameterAst parameterAst)
        {
            return IsParameter(parameterAst, "whatif");
        }

        private static bool IsConfirm(ParameterAst parameterAst)
        {
            return IsParameter(parameterAst, "confirm");
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
