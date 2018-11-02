// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ReturnCorrectTypeDSCFunctions: Check that DSC functions return the correct type.
    /// </summary>
#if !CORECLR
[Export(typeof(IDSCResourceRule))]
#endif
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
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // TODO: Add logic for DSC Resources

            IEnumerable<Ast> functionDefinitionAsts = Helper.Instance.DscResourceFunctions(ast);

            #if !(PSV3||PSV4)

            IEnumerable<TypeDefinitionAst> classes = ast.FindAll(item =>
                item is TypeDefinitionAst
                && ((item as TypeDefinitionAst).IsClass), true).Cast<TypeDefinitionAst>();

            #endif

            foreach (FunctionDefinitionAst func in functionDefinitionAsts)
            {

                #if PSV3 || PSV4

                List<Tuple<string, StatementAst>> outputTypes = FindPipelineOutput.OutputTypes(func);

                #else

                List<Tuple<string, StatementAst>> outputTypes = FindPipelineOutput.OutputTypes(func, classes);

                #endif
                

                if (String.Equals(func.Name, "Set-TargetResource", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Tuple<string, StatementAst> outputType in outputTypes)
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForSetTargetResourceFunctionsDSCError),
                            outputType.Item2.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                    }
                }
                else
                {
                    Dictionary<string, string> returnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    returnTypes["Get-TargetResource"] = typeof(System.Collections.Hashtable).FullName;
                    returnTypes["Test-TargetResource"] = typeof(bool).FullName;

                    foreach (Tuple<string, StatementAst> outputType in outputTypes)
                    {
                        string type = outputType.Item1;

                        if (String.IsNullOrEmpty(type)
                            || String.Equals(typeof(Unreached).FullName, type, StringComparison.OrdinalIgnoreCase)
                            || String.Equals(typeof(Undetermined).FullName, type, StringComparison.OrdinalIgnoreCase)
                            || String.Equals(typeof(object).FullName, type, StringComparison.OrdinalIgnoreCase)
                            || String.Equals(type, returnTypes[func.Name], StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        else
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForGetTestTargetResourceFunctionsDSCResourceError,
                                func.Name, returnTypes[func.Name], type), outputType.Item2.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                        }
                    }
                }
            }
        }

        #if !(PSV3||PSV4)

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
                returnTypes["Test"] = typeof(bool).FullName;
                returnTypes["Get"] = dscClass.Name;

                foreach (var member in dscClass.Members)
                {
                    FunctionMemberAst funcAst = member as FunctionMemberAst;

                    if (funcAst == null || !resourceFunctionNames.Contains(funcAst.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!String.Equals(funcAst.Name, "Set") && !Helper.Instance.AllCodePathReturns(funcAst))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.NotAllCodePathReturnsDSCFunctionsError, funcAst.Name, dscClass.Name),
                            funcAst.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                    }

                    if (String.Equals(funcAst.Name, "Set"))
                    {
                        IEnumerable<Ast> returnStatements = funcAst.FindAll(item => item is ReturnStatementAst, true);
                        foreach (ReturnStatementAst ret in returnStatements)
                        {
                            if (ret.Pipeline != null)
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReturnCorrectTypesForSetFunctionsDSCError, dscClass.Name),
                                    funcAst.Extent, GetName(), DiagnosticSeverity.Information, fileName);
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
                                    ret.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                            }

                            string typeName = Helper.Instance.GetTypeFromReturnStatementAst(funcAst, ret, classes);

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
                                    ret.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                            }
                        }
                    }
                }
            }
        }

        #endif
        

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
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
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



