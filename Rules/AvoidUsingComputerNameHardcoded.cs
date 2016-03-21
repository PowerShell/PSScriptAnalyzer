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
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingComputerNameHardcoded: Check that parameter ComputerName is not hardcoded.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUsingComputerNameHardcoded : AvoidParameterGeneric
    {
        /// <summary>
        /// Condition on the cmdlet that must be satisfied for the error to be raised
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override bool CommandCondition(CommandAst CmdAst)
        {
            return true;
        }

        /// <summary>
        /// Condition on the parameter that must be satisfied for the error to be raised.
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <param name="CeAst"></param>
        /// <returns></returns>
        public override bool ParameterCondition(CommandAst CmdAst, CommandElementAst CeAst)
        {
            if (CeAst is CommandParameterAst)
            {
                CommandParameterAst cmdParamAst = CeAst as CommandParameterAst;

                if (String.Equals(cmdParamAst.ParameterName, "computername", StringComparison.OrdinalIgnoreCase))
                {
                    List<string> localhostRepresentations = new List<string> { "localhost", ".", "::1", "127.0.0.1" };
                    Ast computerNameArgument = GetComputerNameArg(CmdAst, cmdParamAst.Extent.StartOffset);                                        

                    if (null != computerNameArgument)
                    {
                        if (!localhostRepresentations.Contains(computerNameArgument.Extent.Text.ToLower()))
                        {
                            return computerNameArgument is ConstantExpressionAst;
                        }

                        return false;
                    }
                    
                    if (null != cmdParamAst.Argument && !localhostRepresentations.Contains(cmdParamAst.Argument.ToString().Replace("\"", "").Replace("'", "").ToLower()))
                    {
                        return cmdParamAst.Argument is ConstantExpressionAst;
                    }
                }
            }

            return false;
        }

        private Ast GetComputerNameArg(CommandAst CmdAst, int StartIndex)
        {
            int small = int.MaxValue;
            Ast computerNameArg = null;
            foreach (Ast ast in CmdAst.CommandElements)
            {
                if (ast.Extent.StartOffset > StartIndex && ast.Extent.StartOffset < small)
                {
                    computerNameArg = ast;
                    small = ast.Extent.StartOffset;
                }
            }

            return computerNameArg;
        }

        /// <summary>
        /// Retrieves the error message
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override string GetError(string FileName, CommandAst CmdAst)
        {
            if (CmdAst == null)
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidComputerNameHardcodedError, CmdAst.GetCommandName());
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidComputerNameHardcodedName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidComputerNameHardcodedCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidComputerNameHardcodedDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Error;
        }

        /// <summary>
        /// DiagnosticSeverity: Retrieves the severity of the rule of type DiagnosticSeverity: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public override DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Error;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
