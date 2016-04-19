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
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingPing: Avoid Using Ping
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUsingPing : AstVisitor, IScriptRule
    {
        List<DiagnosticRecord> records;
        string fileName;

        /// <summary>
        /// AnalyzeScript: Avoid Using Ping
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            // Rule is applicable only when PowerShell Version is < 5.0, since Test-NetConnection cmdlet was introduced in 5.0
            int majorPSVersion = GetPSMajorVersion(ast);
            if (!(5 > majorPSVersion && 0 < majorPSVersion))
            {             
                ast.Visit(this);
            }

            return records;
        }

        /// <summary>
        /// Checks that write-host command is not used
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitCommand(CommandAst cmdAst)
        {
            if (cmdAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (cmdAst.GetCommandName() != null && String.Equals(cmdAst.GetCommandName(), "ping", StringComparison.OrdinalIgnoreCase))
            {
                if (String.IsNullOrWhiteSpace(fileName))
                {
                    records.Add(new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPingErrorScriptDefinition),
                        cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
                }
                else
                {
                    records.Add(new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPingError,
                        System.IO.Path.GetFileName(fileName)), cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
                }
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// GetPSMajorVersion: Retrieves Major PowerShell Version when supplied using #requires keyword in the script
        /// </summary>
        /// <returns>The name of this rule</returns>
        private int GetPSMajorVersion(Ast ast)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);
            
            foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts)
            {
                if (null != scriptBlockAst.ScriptRequirements && null != scriptBlockAst.ScriptRequirements.RequiredPSVersion)
                {
                    return scriptBlockAst.ScriptRequirements.RequiredPSVersion.Major;
                }
            }

            // return a non valid Major version if #requires -Version is not supplied in the Script
            return -1;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingPingName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPingCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPingDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }


        /// <summary>
        /// GetSeverity:Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
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
