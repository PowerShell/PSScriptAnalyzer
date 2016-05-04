using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    // TODO Use runspace pool
    // TODO Create a new process for the runspace
    // TODO Support for verbose mode    
    public class ModuleDependencyHandler : IDisposable
    {
        #region Private Variables
        private Runspace runspace;
        private readonly string moduleRepository;
        private string tempDirPath;       
        Dictionary<string, PSObject> modulesFound;
        HashSet<string> modulesSavedInTempPath;
        private string localAppdataPath;
        private string pssaAppdataPath;
        private const string symLinkName = "TempModuleDir";
        private const string tempPrefix = "PSSAModules-";
        private string symLinkPath;
        private string oldPSModulePath;
        private string curPSModulePath;

        #endregion Private Variables

        #region Properties
        public string TempModulePath
        {
            get { return tempDirPath; }
        }        

        public Runspace Runspace
        {
            get { return runspace; }
        }

        #endregion Properties

        #region Private Methods
        private static void ThrowIfNull<T>(T obj, string name)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(name);
            }            
        }

        private void SetupCache()
        {
            // check if pssa exists in local appdata
            if (Directory.Exists(pssaAppdataPath))
            {
                // check if there is a link
                if (File.Exists(symLinkPath))
                {
                    tempDirPath = GetTempDirPath(symLinkPath);
                    
                    // check if the temp dir exists
                    if (tempDirPath != null
                        && Directory.Exists(tempDirPath))
                    {
                        SetModulesInTempPath();
                        return;
                    }
                }
                SetupTempDir();                
            }
            else
            {
                Directory.CreateDirectory(pssaAppdataPath);
                SetupTempDir();
            }           
        }

        private void SetModulesInTempPath()
        {
            // we assume the modules have not been tampered with
            foreach (var dir in Directory.EnumerateDirectories(tempDirPath))
            {
                AddModuleName(modulesSavedInTempPath, Path.GetFileName(dir));
            }
        }

        private void AddModuleName(HashSet<string> hashSet, string moduleName)
        {
            hashSet.Add(moduleName.ToLower());
        }

        private bool IsModuleNamePresent(HashSet<string> hashSet, string moduleName)
        {
            return hashSet.Contains(moduleName.ToLower());
        }

        private void SetupTempDir()
        {
            CreateTempDir();
            UpdateSymLinkFile();            
        }

        private void UpdateSymLinkFile()
        {
            File.WriteAllLines(symLinkPath, new string[] { tempDirPath });
        }

        private void CreateTempDir()
        {
            tempDirPath = GetTempDirPath();
            Directory.CreateDirectory(tempDirPath);
        }

        private string GetTempDirPath()
        {
            var tempPathRoot = Path.GetTempPath();
            string tempPath;
            do
            {
                tempPath = Path.Combine(
                    tempPathRoot,
                    tempPrefix + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            } while (Directory.Exists(tempPath));
            return tempPath;
        }

        // Return the first line of the file
        private string GetTempDirPath(string symLinkPath)
        {
            var symLinkLines = File.ReadAllLines(symLinkPath);
            if(symLinkLines.Length != 1)
            {
                return null;
            }
            return symLinkLines[0];
        }
        
        private void CleanUp()
        {
            runspace.Dispose();
            RestorePSModulePath();
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
            modulesSavedInTempPath = new HashSet<string>();
            modulesFound = new Dictionary<string, PSObject>();            
            localAppdataPath = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            // TODO Add PSSA Version in the path
            pssaAppdataPath = Path.Combine(localAppdataPath, "PSScriptAnalyzer");
            symLinkPath = Path.Combine(pssaAppdataPath, symLinkName);
            SetupCache();
            SetupPSModulePath();
        }

        private void SetupPSModulePath()
        {
            oldPSModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            curPSModulePath = oldPSModulePath + ";" + tempDirPath;
            Environment.SetEnvironmentVariable("PSModulePath", curPSModulePath, EnvironmentVariableTarget.Process);
        }

        private void RestorePSModulePath()
        {
            Environment.SetEnvironmentVariable("PSModulePath", oldPSModulePath, EnvironmentVariableTarget.Process);
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
            moduleName = moduleName.ToLower();
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


        public bool ModuleExists(string moduleName)
        {
            throw new NotImplementedException();
        }

        // TODO Do not use find module because it leads to two queries to the server
        // instead use save module and check if it couldn't find the module
        // TODO Add a TrySaveModule method
        public void SaveModule(string moduleName)
        {
            ThrowIfNull(moduleName, "moduleName");
            if (IsModuleNamePresent(modulesSavedInTempPath, moduleName))
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
            AddModuleName(modulesSavedInTempPath, moduleName);             
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