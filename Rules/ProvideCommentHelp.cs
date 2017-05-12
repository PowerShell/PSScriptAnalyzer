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
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideCommentHelp: Analyzes ast to check that cmdlets have help.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class ProvideCommentHelp : SkipTypeDefinition, IScriptRule
    {
        private HashSet<string> exportedFunctions;

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that cmdlets have help.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            DiagnosticRecords.Clear();
            this.fileName = fileName;
            exportedFunctions = Helper.Instance.GetExportedFunction(ast);

            ast.Visit(this);

            return DiagnosticRecords;
        }

        /// <summary>
        /// Visit function and checks that it has comment help
        /// </summary>
        /// <param name="funcAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst funcAst)
        {
            if (funcAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (exportedFunctions.Contains(funcAst.Name))
            {
                if (funcAst.GetHelpContent() == null)
                {
                    // todo create auto correction
                    // todo add option to add help for non exported members
                    // todo add option to set help location
                    DiagnosticRecords.Add(
                        new DiagnosticRecord(
                            string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpError, funcAst.Name),
                            Helper.Instance.GetScriptExtentForFunctionName(funcAst),
                            GetName(),
                            DiagnosticSeverity.Information,
                            fileName,
                            null,
                            GetCorrection(funcAst).ToList()));
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ProvideCommentHelpName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpDescription);
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

        private IEnumerable<CorrectionExtent> GetCorrection(FunctionDefinitionAst funcDefnAst)
        {
            var helpBuilder = new CommentHelpBuilder();

            // todo replace with an extension version
            var paramAsts = (funcDefnAst.Parameters ?? funcDefnAst.Body.ParamBlock?.Parameters)
                                ?? Enumerable.Empty<ParameterAst>();
            foreach (var paramAst in paramAsts)
            {
                helpBuilder.AddParameter(paramAst.Name.VariablePath.UserPath);
            }

            var correctionExtents = new List<CorrectionExtent>();
            yield return new CorrectionExtent(
                funcDefnAst.Extent.StartLineNumber,
                funcDefnAst.Extent.StartLineNumber,
                funcDefnAst.Extent.StartColumnNumber,
                funcDefnAst.Extent.StartColumnNumber,
                helpBuilder.GetCommentHelp(),
                funcDefnAst.Extent.File);
        }

        private class CommentHelpBuilder
        {
            private CommentHelpNode synopsis;
            private CommentHelpNode description;
            private List<CommentHelpNode> parameters;
            private CommentHelpNode example;
            private CommentHelpNode notes;

            public CommentHelpBuilder()
            {
                synopsis = new CommentHelpNode("Synopsis", "Short description");
                description = new CommentHelpNode("Description", "Long description");
                example = new CommentHelpNode("Example", "An example");
                parameters = new List<CommentHelpNode>();
                notes = new CommentHelpNode("Notes", "General notes");
            }

            public void AddParameter(string paramName)
            {
                parameters.Add(new ParameterHelpNode(paramName, "Parameter description"));
            }

            // todo add option for comment type
            public string GetCommentHelp()
            {
                var sb = new StringBuilder();
                sb.AppendLine("<#");
                sb.AppendLine(this.ToString());
                sb.Append("#>");
                return sb.ToString();
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine(synopsis.ToString()).AppendLine();
                sb.AppendLine(description.ToString()).AppendLine();
                foreach (var parameter in parameters)
                {
                    sb.AppendLine(parameter.ToString()).AppendLine();
                }

                sb.AppendLine(example.ToString()).AppendLine();
                sb.Append(notes.ToString());
                return sb.ToString();
            }
            private class CommentHelpNode
            {
                public CommentHelpNode(string nodeName, string description)
                {
                    Name = nodeName;
                    Description = description;
                }

                public string Name { get; }
                public string Description { get; set; }

                public override string ToString()
                {
                    var sb = new StringBuilder();
                    sb.Append(".").AppendLine(Name.ToUpper());
                    if (!String.IsNullOrWhiteSpace(Description))
                    {
                        sb.Append(Description);
                    }

                    return sb.ToString();
                }
            }

            private class ParameterHelpNode : CommentHelpNode
            {
                public ParameterHelpNode(string parameterName, string parameterDescription)
                    : base("Parameter", parameterDescription)
                {
                    ParameterName = parameterName;
                }

                public string ParameterName { get; }

                public override string ToString()
                {
                    var sb = new StringBuilder();
                    sb.Append(".").Append(Name.ToUpper()).Append(" ").AppendLine(ParameterName);
                    if (!String.IsNullOrWhiteSpace(Description))
                    {
                        sb.Append(Description);
                    }

                    return sb.ToString();
                }
            }
        }
    }
}




