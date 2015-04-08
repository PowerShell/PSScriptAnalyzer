using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseIdenticalMandatoryParametersDSC: Check that the Get/Test/Set TargetResource
    /// have identical mandatory parameters.
    /// </summary>
    [Export(typeof(IDSCResourceRule))]
    public class UseIdenticalMandatoryParametersDSC : IDSCResourceRule
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
            List<string> expectedTargetResourceFunctionNames = new List<string>(new string[] { "Set-TargetResource", "Test-TargetResource", "Get-TargetResource" });

            IEnumerable<Ast> functionDefinitionAsts = Helper.Instance.DscResourceFunctions(ast);

            // Dictionary to keep track of Mandatory parameters and their presence in Get/Test/Set TargetResource cmdlets
            Dictionary<string, List<string>> mandatoryParameters = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Loop through Set/Test/Get TargetResource DSC cmdlets
            foreach (FunctionDefinitionAst functionDefinitionAst in functionDefinitionAsts)
            {
                IEnumerable<Ast> funcParamAsts = functionDefinitionAst.FindAll(item => item is ParameterAst, true);

                // Loop through the parameters for each cmdlet
                foreach (ParameterAst paramAst in funcParamAsts)
                {
                    // Loop through the attributes for each of those cmdlets
                    foreach (var paramAstAttributes in paramAst.Attributes)
                    {
                        if (paramAstAttributes is AttributeAst)
                        {
                            var namedArguments = (paramAstAttributes as AttributeAst).NamedArguments;
                            if (namedArguments != null)
                            {
                                // Loop through the named attribute arguments for each parameter
                                foreach (NamedAttributeArgumentAst namedArgument in namedArguments)
                                {
                                    // Look for Mandatory parameters
                                    if (String.Equals(namedArgument.ArgumentName, "mandatory", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Covers Case - [Parameter(Mandatory)] and [Parameter(Mandatory)=$true]
                                        if (namedArgument.ExpressionOmitted || (!namedArgument.ExpressionOmitted && String.Equals(namedArgument.Argument.Extent.Text, "$true", StringComparison.OrdinalIgnoreCase)))
                                        {                                            
                                            if (mandatoryParameters.ContainsKey(paramAst.Name.VariablePath.UserPath))
                                            {
                                                mandatoryParameters[paramAst.Name.VariablePath.UserPath].Add(functionDefinitionAst.Name);
                                            }
                                            else
                                            {
                                                List<string> functionNames = new List<string>();
                                                functionNames.Add(functionDefinitionAst.Name);
                                                mandatoryParameters.Add(paramAst.Name.VariablePath.UserPath, functionNames);
                                            }                                            
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Get the mandatory parameter names that do not appear in all the DSC Resource cmdlets 
            IEnumerable<string> paramNames = mandatoryParameters.Where(x => x.Value.Count < expectedTargetResourceFunctionNames.Count).Select(x => x.Key); 
           
            if (paramNames.Count() > 0)
            {                
                foreach (string paramName in paramNames)
                {
                    List<string> functionsNotContainingParam = expectedTargetResourceFunctionNames.Except(mandatoryParameters[paramName]).ToList();
                    yield return new DiagnosticRecord(string.Format(CultureInfo.InvariantCulture, Strings.UseIdenticalMandatoryParametersDSCError, paramName, string.Join(", ", functionsNotContainingParam.ToArray())),
                                    ast.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                }                   
                
            }
            
        }

        /// <summary>
        /// AnalyzeDSCClass: This function returns nothing in the case of dsc class.
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName)
        {
            // For DSC Class based resource, this rule is N/A, since the Class Properties 
            // are declared only once and available to Get(), Set(), Test() functions
            return Enumerable.Empty<DiagnosticRecord>();
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {            
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseIdenticalMandatoryParametersDSCName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalMandatoryParametersDSCCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalMandatoryParametersDSCDescription);
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