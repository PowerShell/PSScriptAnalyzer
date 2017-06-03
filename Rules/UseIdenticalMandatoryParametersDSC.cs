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
using Microsoft.PowerShell.DesiredStateConfiguration.Internal;
using System.IO;
using Microsoft.Management.Infrastructure;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseIdenticalMandatoryParametersDSC: Check that the Get/Test/Set TargetResource
    /// have identical mandatory parameters.
    /// </summary>
#if !CORECLR
    [Export(typeof(IDSCResourceRule))]
#endif
    public class UseIdenticalMandatoryParametersDSC : IDSCResourceRule
    {
        private bool isDSCClassCacheInitialized = false;

        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // todo write tests for same
            // todo update documentation
            var keys = GetKeys(fileName);

            // Loop through Set/Test/Get TargetResource DSC cmdlets
            foreach (FunctionDefinitionAst functionDefinitionAst in Helper.Instance.DscResourceFunctions(ast))
            {
                var manParams = new HashSet<string>(
                    GetMandatoryParameters(functionDefinitionAst).Select(p => p.Name.VariablePath.UserPath),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var key in keys)
                {
                    if (!manParams.Contains(key))
                    {
                        yield return new DiagnosticRecord(
                         string.Format(
                             CultureInfo.InvariantCulture,
                             Strings.UseIdenticalMandatoryParametersDSCError,
                             key,
                             functionDefinitionAst.Name),
                         Helper.Instance.GetScriptExtentForFunctionName(functionDefinitionAst),
                         GetName(),
                         DiagnosticSeverity.Error,
                         fileName);
                    }
                }
            }
        }

        private IEnumerable<ParameterAst> GetMandatoryParameters(FunctionDefinitionAst functionDefinitionAst)
        {
            return functionDefinitionAst.GetParameterAsts()?.Where(IsParameterMandatory) ??
                        Enumerable.Empty<ParameterAst>();
        }

        private bool IsParameterMandatory(ParameterAst paramAst)
        {
            var attrAsts = from attr in paramAst.Attributes
                           where IsParameterAttribute(attr) && attr is AttributeAst
                           select (AttributeAst)attr;

            return attrAsts.Any(a => a.NamedArguments.Any(IsNamedAttributeArgumentMandatory));
        }

        private bool IsParameterAttribute(AttributeBaseAst attributeBaseAst)
        {
            return attributeBaseAst.TypeName.GetReflectionType().Name.Equals("ParameterAttribute");
        }

        private bool IsNamedAttributeArgumentMandatory(NamedAttributeArgumentAst namedAttrArgAst)
        {
            return namedAttrArgAst.ArgumentName.Equals("mandatory", StringComparison.OrdinalIgnoreCase) &&
                    namedAttrArgAst.GetValue();
        }

        private IEnumerable<string> GetKeys(string fileName)
        {
            var moduleInfo = GetModuleInfo(fileName);
            var emptyArray = new string[0];
            if (moduleInfo == null)
            {
                return emptyArray;
            }

            var mofFilepath = GetMofFilepath(fileName);
            if (mofFilepath == null)
            {
                return emptyArray;
            }

            var errors = new System.Collections.ObjectModel.Collection<Exception>();
            var keys = new List<string>();
            List<CimClass> cimClasses = null;
            try
            {
                if (!isDSCClassCacheInitialized)
                {
                    DscClassCache.Initialize();
                    isDSCClassCacheInitialized = true;
                }

                cimClasses = DscClassCache.ImportClasses(mofFilepath, moduleInfo, errors);
            }
            catch
            {
                // todo log the error
            }

            return cimClasses?
                    .FirstOrDefault()?
                    .CimClassProperties?
                    .Where(p => p.Flags.HasFlag(CimFlags.Key))
                    .Select(p => p.Name)
                    .ToArray() ??
                    emptyArray;
        }

        private string GetMofFilepath(string filePath)
        {
            var mofFilePath = Path.Combine(
                    Path.GetDirectoryName(filePath),
                    Path.GetFileNameWithoutExtension(filePath)) + ".schema.mof";

            return File.Exists(mofFilePath) ? mofFilePath : null;
        }

        private Tuple<string, Version> GetModuleInfo(string fileName)
        {
            var moduleManifest = GetModuleManifest(fileName);
            if (moduleManifest == null)
            {
                return null;
            }

            var moduleName = Path.GetFileNameWithoutExtension(moduleManifest.Name);
            Token[] tokens;
            ParseError[] parseErrors;
            var ast = Parser.ParseFile(moduleManifest.FullName, out tokens, out parseErrors);
            if ((parseErrors != null && parseErrors.Length > 0) || ast == null)
            {
                return null;
            }

            var foundAst = ast.Find(x => x is HashtableAst, false);
            if (foundAst == null)
            {
                return null;
            }

            var moduleVersionKvp = ((HashtableAst)foundAst).KeyValuePairs.FirstOrDefault(t =>
            {
                var keyAst = t.Item1 as StringConstantExpressionAst;
                return keyAst != null &&
                    keyAst.Value.Equals("ModuleVersion", StringComparison.OrdinalIgnoreCase);
            });

            if (moduleVersionKvp == null)
            {
                return null;
            }

            var valueAst = moduleVersionKvp.Item2.Find(a => a is StringConstantExpressionAst, false);
            var versionText = valueAst == null ? null : ((StringConstantExpressionAst)valueAst).Value;
            Version version;
            Version.TryParse(versionText, out version); // this handles null so no need to check versionText
            return version == null ? null : Tuple.Create(moduleName, version);
        }

        private FileInfo GetModuleManifest(string fileName)
        {
            var moduleRoot = Directory.GetParent(fileName)?.Parent?.Parent;
            if (moduleRoot != null)
            {
                var files = moduleRoot.GetFiles("*.psd1");
                if (files != null && files.Length == 1)
                {
                    return files[0];
                }
            }

            return null;
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
            return string.Format(CultureInfo.CurrentCulture, Strings.DSCSourceName);
        }
    }

}



