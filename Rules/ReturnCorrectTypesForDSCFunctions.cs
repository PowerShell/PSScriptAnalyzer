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
    /// ReturnCorrectTypeDSCFunctions: Check that DSC functions return the correct type.
    /// </summary>
    [Export(typeof(IDSCResourceRule))]
    public class ReturnCorrectTypesForDSCFunctions : IDSCResourceRule
    {
        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName)
        {
            // TODO: Add logic for DSC Resources
            return Enumerable.Empty<DiagnosticRecord>();
        }

        /// <summary>
        /// AnalyzeDSCClass: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);


            IEnumerable<TypeDefinitionAst> classes = ast.FindAll(item =>
                item is TypeDefinitionAst
                && ((item as TypeDefinitionAst).IsClass), true).Cast<TypeDefinitionAst>();

            IEnumerable<TypeDefinitionAst> dscClasses = classes.Where(item => (item as TypeDefinitionAst).Attributes.Any(attr => String.Equals("DSCResource", attr.TypeName.FullName, StringComparison.OrdinalIgnoreCase)));

            List<string> resourceFunctionNames = new List<string>(new string[] { "Test", "Get", "Set" });

            foreach (TypeDefinitionAst dscClass in dscClasses)
            {
                Dictionary<string, string> returnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                returnTypes["Get"] = dscClass.Name;
                returnTypes["Test"] = typeof(bool).FullName;

                foreach (var member in dscClass.Members)
                {
                    FunctionMemberAst funcAst = member as FunctionMemberAst;

                    if (funcAst == null || !resourceFunctionNames.Contains(funcAst.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Helper.Instance.InitializeVariableAnalysis(funcAst);

                    if (!String.Equals(funcAst.Name, "Set") && !Helper.Instance.AllCodePathReturns(funcAst))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.NotAllCodePathReturnsDSCFunctionsError, funcAst.Name, dscClass.Name),
                            funcAst.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
                    }

                    if (String.Equals(funcAst.Name, "Set"))
                    {
                        IEnumerable<Ast> returnStatements = funcAst.FindAll(item => item is ReturnStatementAst, true);
                        foreach (ReturnStatementAst ret in returnStatements)
                        {
                            if (ret.Pipeline != null)
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForSetFunctionsDSCError, dscClass.Name),
                                    funcAst.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
                            }
                        }
                    }

                    if (returnTypes.ContainsKey(funcAst.Name))
                    {
                        IEnumerable<Ast> returnStatements = funcAst.FindAll(item => item is ReturnStatementAst, true);
                        Type type = funcAst.ReturnType.TypeName.GetReflectionType();

                        foreach (ReturnStatementAst ret in returnStatements)
                        {
                            if (ret.Pipeline == null)
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForDSCFunctionsNoTypeError,
                                    funcAst.Name, dscClass.Name, returnTypes[funcAst.Name]),
                                    ret.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
                            }

                            string typeName = Helper.Instance.GetTypeFromReturnStatementAst(funcAst, ret, classes, ast);

                            // This also includes the case of return $this because the type of this is unreached.
                            if (String.IsNullOrEmpty(typeName)
                                || String.Equals(typeof(Unreached).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                                || String.Equals(typeof(Undetermined).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                                || String.Equals(typeof(object).FullName, typeName, StringComparison.OrdinalIgnoreCase)
                                || String.Equals(returnTypes[funcAst.Name], typeName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            else
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForDSCFunctionsWrongTypeError,
                                    funcAst.Name, dscClass.Name, returnTypes[funcAst.Name], typeName),
                                    ret.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ReturnCorrectTypeDSCFunctionsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForDSCFunctionsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForDSCFunctionsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DSCSourceName);
        }
    }

}