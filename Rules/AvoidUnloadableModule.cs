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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.IO;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUnloadableModule: Run Import-Module on parent folder to check whether the module is loaded.
    /// </summary>
    [Export(typeof (IScriptRule))]
    public class AvoidUnloadableModule : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Run Import-Module on parent folder to check whether the module is loaded. From the IScriptRule interface.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            DirectoryInfo moduleFolder = Directory.GetParent(fileName);

            if (!String.Equals(moduleFolder.FullName, Path.GetPathRoot(fileName)))
            {
                Regex reg = new Regex(String.Format(CultureInfo.CurrentCulture, "{0}\\.(dll|psm1|psd1|cdxml|xaml)",
                    Regex.Escape(Path.Combine(moduleFolder.FullName, moduleFolder.Name))));
                var moduleFiles = moduleFolder.GetFiles().Where(file => reg.Match(file.FullName).Success).ToList();

                if (moduleFiles != null && moduleFiles.Count > 0)
                {
                    bool moduleValid = true;
                    using (var ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
                    {
                        try
                        {
                            ps.AddCommand("Import-Module");
                            ps.AddParameter("Name", moduleFolder.FullName);
                            ps.Invoke();

                            if (ps != null && ps.HadErrors)
                            {
                                moduleValid = false;
                            }
                        }
                        catch (Exception)
                        {
                            //Catch the exception thrown by ps.Invoke();
                            moduleValid = false;
                        }

                        if (!moduleValid)
                        { 
                            yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture,
                                Strings.AvoidUnloadableModuleError, moduleFolder.Name, Path.GetFileName(fileName)),
                                ast.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUnloadableModuleName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUnloadableModuleCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUnloadableModuleDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
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
