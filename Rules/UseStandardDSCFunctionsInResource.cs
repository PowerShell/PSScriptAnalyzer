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
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseStandardDSCFunctionsInResource: 
    /// </summary>
    [Export(typeof(IDSCResourceRule))]
    public class UseStandardDSCFunctionsInResource : IDSCResourceRule
    {
        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);
                        
            // Expected TargetResource functions in the DSC Resource module
            List<string> expectedTargetResourceFunctionNames = new List<string>(new string[]  { "Get-TargetResource", "Set-TargetResource", "Test-TargetResource" });

            // Retrieve a list of Asts where the function name contains TargetResource            
            IEnumerable<Ast> functionDefinitionAsts = (ast.FindAll(dscAst => dscAst is FunctionDefinitionAst && ((dscAst as FunctionDefinitionAst).Name.IndexOf("targetResource", StringComparison.CurrentCultureIgnoreCase) != -1), true));

            List<string> targetResourceFunctionNamesInAst = new List<string>();
            foreach (FunctionDefinitionAst functionDefinitionAst in functionDefinitionAsts)
            {
                targetResourceFunctionNamesInAst.Add(functionDefinitionAst.Name);
            }
            
            foreach (string expectedTargetResourceFunctionName in expectedTargetResourceFunctionNames)
            {
                // If the Ast does not contain the expected functions, provide a Rule violation message
                if (!targetResourceFunctionNamesInAst.Contains(expectedTargetResourceFunctionName, StringComparer.CurrentCultureIgnoreCase))
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseStandardDSCFunctionsInResourceError, expectedTargetResourceFunctionName),
                        ast.Extent, GetName(), DiagnosticSeverity.Information, fileName);      
                }
            }
        }

        /// <summary>
        /// AnalyzeDSCClass: Analyzes dsc classes and the file and check that they have get, set and test
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

#if PSV3
            return null;
#else
            List<string> resourceFunctionNames = new List<string>(new string[] {"Test", "Get", "Set"});

            IEnumerable<Ast> dscClasses = ast.FindAll(item =>
                item is TypeDefinitionAst
                && ((item as TypeDefinitionAst).IsClass)
                && (item as TypeDefinitionAst).Attributes.Any(attr => String.Equals("DSCResource", attr.TypeName.FullName, StringComparison.OrdinalIgnoreCase)), true);

            foreach (TypeDefinitionAst dscClass in dscClasses)
            {
                IEnumerable<Ast> functions = dscClass.Members.Where(member => member is FunctionMemberAst);

                foreach (string resourceFunctionName in resourceFunctionNames)
                {
                    if (!functions.Any(function => String.Equals(resourceFunctionName, (function as FunctionMemberAst).Name)))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseStandardDSCFunctionsInClassError, resourceFunctionName),
                            dscClass.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {            
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseStandardDSCFunctionsInResourceName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseStandardDSCFunctionsInResourceCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseStandardDSCFunctionsInResourceDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
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
            return RuleSeverity.Error;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture,Strings.DSCSourceName);
        }
    }

}