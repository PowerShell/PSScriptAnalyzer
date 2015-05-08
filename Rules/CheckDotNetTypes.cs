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
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;
using NanoServerCompliance;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// CheckDotNetTypes: Check if dot net type, and its methods and properties are available on CoreCLR.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class CheckDotNetTypes : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if .dot net type is supported on CoreCLR.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            //Initialize new dictionaries to clear up stored values.
            Dictionary<string, List<string>> typeMethod = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> typeProperty = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> typeFields = new Dictionary<string, List<string>>();

            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);
            IEnumerable<Ast> funcs = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            foreach (FunctionDefinitionAst func in funcs)
            {
                IEnumerable<Ast> memberExpressions = func.FindAll(testAst => testAst is MemberExpressionAst, true);

                

                //Get the type of the variable before checking if the type has the underliend methods/properties
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ReflectionTypeAnalysis.Instance.GetTypeFromAssembly(path +"../../Engine/ReferenceAssembly");
                typeMethod = ReflectionTypeAnalysis.Instance.typeMethod;
                typeProperty = ReflectionTypeAnalysis.Instance.typeProperties;
                typeFields = ReflectionTypeAnalysis.Instance.typeFields;

                foreach (MemberExpressionAst member in memberExpressions)
                {
                    if (member.Expression is VariableExpressionAst)
                    {
                        VariableExpressionAst varAst = member.Expression as VariableExpressionAst;

                        //    //Get the type of the variable before checking if the type has the underliend methods/properties
                        string type = Helper.Instance.GetVariableTypeFromAnalysis(varAst, func);
                        string field = member.Member.Extent.Text;

                        if (!typeMethod.ContainsKey(type))
                        {
                            if (
                                !string.IsNullOrEmpty(type) &&
                                !string.Equals("Microsoft.Windows.Powershell.ScriptAnalyzer.Unreached", type,
                                    StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals("Microsoft.Windows.Powershell.ScriptAnalyzer.Undetermined", type,
                                    StringComparison.OrdinalIgnoreCase) &&
                                !type.Contains("[]"))
                            {
                                yield return
                                    new DiagnosticRecord(
                                        string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeUnavailableError,
                                            type),
                                        varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
                        else
                        {
                            if (!typeMethod[type].Contains(field, StringComparer.OrdinalIgnoreCase) && !typeProperty[type].Contains(field, StringComparer.OrdinalIgnoreCase)
                                && !typeFields[type].Contains(field,StringComparer.OrdinalIgnoreCase))
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeMemberUnavailableError, type, field),
                                varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
                    }

                    else if (member.Expression is TypeExpressionAst)
                    {
                        TypeExpressionAst typeAst = member.Expression as TypeExpressionAst;
                        Type type = typeAst.TypeName.GetReflectionType();
                        string field = member.Member.Extent.Text;
                        string typeName = type.FullName;
                        if (!typeMethod.ContainsKey(typeName))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeUnavailableError, type),
                                typeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                        else
                        {
                            if (!typeMethod[typeName].Contains(field, StringComparer.OrdinalIgnoreCase) && !typeProperty[typeName].Contains(field, StringComparer.OrdinalIgnoreCase)
                                && !typeFields[typeName].Contains(field, StringComparer.OrdinalIgnoreCase))
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeMemberUnavailableError, type, field),
                                typeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
      
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
            return string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeUnavailableName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeUnavailableCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>  
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DotNetTypeUnavailableDescription);
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
            return RuleSeverity.Warning;
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
