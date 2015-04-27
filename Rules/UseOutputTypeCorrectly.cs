//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideCommentHelp: Checks that objects return in a cmdlet have their types declared in OutputType Attribute
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseOutputTypeCorrectly : SkipTypeDefinition, IScriptRule
    {
        private IEnumerable<TypeDefinitionAst> _classes;

        /// <summary>
        /// AnalyzeScript: Checks that objects return in a cmdlet have their types declared in OutputType Attribute
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            DiagnosticRecords.Clear();
            this.fileName = fileName;

            _classes = ast.FindAll(item => item is TypeDefinitionAst && ((item as TypeDefinitionAst).IsClass), true).Cast<TypeDefinitionAst>();

            ast.Visit(this);

            return DiagnosticRecords;
        }

        /// <summary>
        /// Visit function and checks that it is a cmdlet. If yes, then checks that any object returns must have a type declared
        /// in the output type (the only exception is if the type is object)
        /// </summary>
        /// <param name="funcAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst funcAst)
        {
            if (funcAst == null || funcAst.Body == null || funcAst.Body.ParamBlock == null
                || funcAst.Body.ParamBlock.Attributes == null || funcAst.Body.ParamBlock.Attributes.Count == 0
                || !funcAst.Body.ParamBlock.Attributes.Any(attr => attr.TypeName.GetReflectionType() == typeof(CmdletBindingAttribute)))
            {
                return AstVisitAction.Continue;
            }

            HashSet<string> outputTypes = new HashSet<string>();

            foreach (AttributeAst attrAst in funcAst.Body.ParamBlock.Attributes)
            {
                if (attrAst.TypeName != null && attrAst.TypeName.GetReflectionType() == typeof(OutputTypeAttribute)
                    && attrAst.PositionalArguments != null)
                {
                    foreach (ExpressionAst expAst in attrAst.PositionalArguments)
                    {
                        if (expAst is StringConstantExpressionAst)
                        {
                            Type type = Type.GetType((expAst as StringConstantExpressionAst).Value);
                            if (type != null)
                            {
                                outputTypes.Add(type.FullName);
                            }
                        }
                        else
                        {
                            TypeExpressionAst typeAst = expAst as TypeExpressionAst;
                            if (typeAst != null && typeAst.TypeName != null)
                            {
                                if (typeAst.TypeName.GetReflectionType() != null)
                                {
                                    outputTypes.Add(typeAst.TypeName.GetReflectionType().FullName);
                                }
                                else
                                {
                                    outputTypes.Add(typeAst.TypeName.FullName);
                                }
                            }
                        }
                    }
                }
            }

            List<Tuple<string, StatementAst>> returnTypes = FindPipelineOutput.OutputTypes(funcAst, _classes);

            foreach (Tuple<string, StatementAst> returnType in returnTypes)
            {
                string typeName = returnType.Item1;

                if (String.IsNullOrEmpty(typeName)
                    || String.Equals(typeof(Unreached).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(typeof(Undetermined).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(typeof(object).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                    || outputTypes.Contains(typeName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    DiagnosticRecords.Add(new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseOutputTypeCorrectlyError,
                        funcAst.Name, typeName), returnType.Item2.Extent, GetName(), DiagnosticSeverity.Information, fileName));
                }
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseOutputTypeCorrectlyName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseOutputTypeCorrectlyCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseOutputTypeCorrectlyDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
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
