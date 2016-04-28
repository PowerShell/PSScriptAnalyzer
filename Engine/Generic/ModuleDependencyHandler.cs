using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    // TODO Use runspace pool
    // TODO Create a new process for the runspace
    public class ModuleDependencyHandler : IDisposable
    {
        #region Private Variables
        private Runspace runspace;
        private readonly string moduleRepository;
        private string tempDirPath;
        Dictionary<string, PSObject> modulesFound;
        HashSet<string> modulesSaved;
        private string oldPSModulePath;
        private string currentModulePath;

        #endregion Private Variables

        #region Properties
        public string ModulePath
        {
            get { return tempDirPath; }
        }        

        public Runspace Runspace
        {
            get { return runspace; }
        }

        #endregion

        #region Private Methods
        private static void ThrowIfNull<T>(T obj, string name)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(name);
            }            
        }
        private void SetupTempDir()
        {
            //var tempPath = Path.GetTempPath();
            //do
            //{
            //    tempDirPath = Path.Combine(
            //        tempPath, 
            //        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            //} while (Directory.Exists(tempDirPath));
            //Directory.CreateDirectory(tempDirPath);
            tempDirPath = "C:\\Users\\kabawany\\tmp\\modules\\";
        }

        private void RemoveTempDir()
        {
            //Directory.Delete(tempDirPath, true);
        }

        private void SetupPSModulePath()
        {
            oldPSModulePath = Environment.GetEnvironmentVariable("PSModulePath", EnvironmentVariableTarget.Process);
            var sb = new StringBuilder();
            sb.Append(oldPSModulePath)
                .Append(Path.DirectorySeparatorChar)
                .Append(tempDirPath);
            currentModulePath = sb.ToString();
        }
        
        private void CleanUp()
        {
            runspace.Dispose();
            RemoveTempDir();            
            RestorePSModulePath();
        }

        private void RestorePSModulePath()
        {
            Environment.SetEnvironmentVariable("PSModulePath", oldPSModulePath, EnvironmentVariableTarget.Process);
        }

        private void SaveModule(PSObject module)
        {
            ThrowIfNull(module, "module");
            // TODO validate module
            var ps = System.Management.Automation.PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddCommand("Save-Module")
                .AddParameter("Path", tempDirPath)
                .AddParameter("InputObject", module);
            ps.Invoke();
        }

        #endregion Private Methods

        #region Public Methods

        public ModuleDependencyHandler()
        {
            runspace = null;
            moduleRepository = "PSGallery";
            modulesSaved = new HashSet<string>();
            modulesFound = new Dictionary<string, PSObject>();
            SetupTempDir();
            //SetupPSModulePath();           
        }

        public ModuleDependencyHandler(Runspace runspace) : this()
        {
            ThrowIfNull(runspace, "runspace");
            this.runspace = runspace;
            this.runspace.Open();
        }


        public void SetupDefaultRunspace()
        {
            runspace = RunspaceFactory.CreateRunspace();
        }

        public void SetupDefaultRunspace(Runspace runspace)
        {
            ThrowIfNull(runspace, "runspace");
            if (runspace != null)
            {
                this.runspace = runspace;
            }
            Runspace.DefaultRunspace = this.runspace;
        }

        public PSObject FindModule(string moduleName)
        {
            ThrowIfNull(moduleName, "moduleName");
            if (modulesFound.ContainsKey(moduleName))
            {
                return modulesFound[moduleName];
            }
            var ps = System.Management.Automation.PowerShell.Create();
            Collection<PSObject> modules = null;            
            ps.Runspace = runspace;
            ps.AddCommand("Find-Module", true)
                .AddParameter("Name", moduleName)
                .AddParameter("Repository", moduleRepository);
            modules = ps.Invoke<PSObject>();
            
            if (modules == null)
            {
                return null;
            }
            var module = modules.FirstOrDefault();
            if (module == null )
            {
                return null;
            }
            modulesFound.Add(moduleName, module);
            return module;
        }

        public void SaveModule(string moduleName)
        {
            ThrowIfNull(moduleName, "moduleName");
            if (modulesSaved.Contains(moduleName))
            {
                return;
            }
            var module = FindModule(moduleName);
            if (module == null)
            {
                throw new ItemNotFoundException(
                    string.Format(
                        "Cannot find {0} in {1} repository.",
                        moduleName,
                        moduleRepository));
            }            
            SaveModule(module);
            modulesSaved.Add(moduleName);
        }

        public static string GetModuleNameFromErrorExtent(ParseError error, ScriptBlockAst ast)
        {
            ThrowIfNull(error, "error");
            ThrowIfNull(ast, "ast");
            var statement = ast.Find(x => x.Extent.Equals(error.Extent), true);
            var dynamicKywdAst = statement as DynamicKeywordStatementAst;
            if (dynamicKywdAst == null)
            {
                return null;
            }
            // check if the command name is import-dscmodule
            // right now we handle only the following form
            // Import-DSCResource -ModuleName xActiveDirectory
            if (dynamicKywdAst.CommandElements.Count != 3)
            {
                return null;
            }

            var dscKeywordAst = dynamicKywdAst.CommandElements[0] as StringConstantExpressionAst;
            if (dscKeywordAst == null || !dscKeywordAst.Value.Equals("Import-DscResource", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var paramAst = dynamicKywdAst.CommandElements[1] as CommandParameterAst;
            if (paramAst == null || !paramAst.ParameterName.Equals("ModuleName", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var paramValAst = dynamicKywdAst.CommandElements[2] as StringConstantExpressionAst;
            if (paramValAst == null)
            {
                return null;
            }

            return paramValAst.Value;
        }

        public void Dispose()
        {
            CleanUp();
        }

        #endregion Public Methods
    }
}
