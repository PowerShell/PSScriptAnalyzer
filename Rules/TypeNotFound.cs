using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// TypeNotFound: Check that all the types in the script are correct.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class TypeNotFound : IScriptRule
    {
        /// <summary>
        /// TypeNotFound: Check that all the types in the script are correct.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<string> types = getTypesFromAppDomain(ast);
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is AttributeBaseAst, true);

            // From Jason: 
            // It would be nice to discover dependencies automatically.  
            // We can do this to some extent, e.g. explicit calls to Import-Module, Add-Type, and calls to [System.Reflection.Assembly]::Load* indicate a dependency, 
            // but the arguments to these cmdlets/methods aren’t always constant, so we need help from the user.

            // Try to retrieve dll path/name from StringConstantExpressionAst?

            foreach (Ast foundAst in foundAsts)
            {
                AttributeBaseAst attrAst = (AttributeBaseAst)foundAst;
                string typeName = attrAst.TypeName.Name;
                if (typeName.EndsWith("[]"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 2);
                }

                if (types.Count<string>(item => item.EndsWith(
                    typeName, StringComparison.OrdinalIgnoreCase)
                    || item.EndsWith(typeName + "Attribute", StringComparison.OrdinalIgnoreCase)) == 0)
                {
                    yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.TypeNotFoundError, attrAst.TypeName.Name),
                        attrAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        private IEnumerable<string> getTypesFromAppDomain(Ast ast)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    yield return type.FullName;
                }
            }

        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.TypeNotFoundName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.TypeNotFoundCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.TypeNotFoundDescription);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
