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
    // TODO Support for verbose mode    
    public class ModuleDependencyHandler : IDisposable
    {
        #region Private Variables
        private Runspace runspace;
        private string moduleRepository;
        private string tempPath; // path to the user temporary directory 
        private string tempModulePath; // path to temp directory containing modules
        Dictionary<string, PSObject> modulesFound;
        private string localAppdataPath;
        private string pssaAppDataPath;
        private const string symLinkName = "TempModuleDir";
        private const string tempPrefix = "PSSAModules-";
        private string symLinkPath;
        private string oldPSModulePath;
        private string curPSModulePath;
        #endregion Private Variables

        #region Properties
        /// <summary>
        /// Path where the object stores the modules
        /// </summary>
        public string TempModulePath
        {
            get { return tempModulePath; }
        }

        /// <summary>
        /// Temporary path of the current user scope
        /// </summary>
        public string TempPath
        {
            get
            {
                return tempPath;
            }
            // it must be set only during initialization
            private set
            {
                tempPath
                    = (string.IsNullOrEmpty(value)
                        || string.IsNullOrWhiteSpace(value))
                    ? Environment.GetEnvironmentVariable("TEMP")
                    : value;
            }

        }

        /// <summary>
        /// Local App Data path
        /// </summary>
        public string LocalAppDataPath
        {
            get
            {
                return localAppdataPath;
            }
            private set
            {
                localAppdataPath 
                    = (string.IsNullOrEmpty(value)
                        || string.IsNullOrWhiteSpace(value))
                    ? Environment.GetEnvironmentVariable("LOCALAPPDATA")
                    : value;
            }
        }        

        /// <summary>
        /// Module Respository
        /// By default it is PSGallery
        /// </summary>
        public string ModuleRepository
        {
            get
            {
                return moduleRepository;
            }
            set
            {
                moduleRepository
                    = (string.IsNullOrEmpty(value)
                        || string.IsNullOrWhiteSpace(value))
                    ? "PSGallery"
                    : value;
            }
        }

        /// <summary>
        /// Local App data of PSSScriptAnalyzer
        /// </summary>
        public string PSSAAppDataPath
        {
            get
            {
                return pssaAppDataPath;
            }
            private set
            {
                var leaf
                    = (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    ? "PSScriptAnalyzer"
                    : value;
                pssaAppDataPath = Path.Combine(LocalAppDataPath, leaf);
            }
        }        

        /// <summary>
        /// Module Paths
        /// </summary>
        public string PSModulePath
        {
            get { return curPSModulePath; }
        }
       
        /// <summary>
        /// Runspace in which the object invokes powershell cmdlets
        /// </summary>
        public Runspace Runspace    
        {
            get { return runspace; }
            set { runspace = value; }
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

        private void SetupPSSAAppData()
        {
            // check if pssa exists in local appdata
            if (Directory.Exists(pssaAppDataPath))
            {
                // check if there is a link
                if (File.Exists(symLinkPath))
                {
                    tempModulePath = GetTempModulePath(symLinkPath);
                    
                    // check if the temp dir exists
                    if (tempModulePath != null
                        && Directory.Exists(tempModulePath))
                    {
                        return;
                    }
                }                          
            }
            else
            {
                Directory.CreateDirectory(pssaAppDataPath);                
            }
            SetupTempDir();
        }

        private bool IsModulePresent(string moduleName)
        {
            foreach (var dir in Directory.EnumerateDirectories(TempModulePath))
            {
                if (moduleName.Equals(dir, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void SetupTempDir()
        {
            tempModulePath = GetPSSATempDirPath();
            Directory.CreateDirectory(tempModulePath);
            File.WriteAllLines(symLinkPath, new string[] { tempModulePath });
        }

        private string GetPSSATempDirPath()
        {
            string path;
            do
            {
                path = Path.Combine(
                    tempPath,
                    tempPrefix + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            } while (Directory.Exists(path));
            return path;
        }

        // Return the first line of the file
        private string GetTempModulePath(string symLinkPath)
        {
            var symLinkLines = File.ReadAllLines(symLinkPath);
            if(symLinkLines.Length != 1)
            {
                return null;
            }
            return symLinkLines[0];
        }

        private void SaveModule(PSObject module)
        {
            ThrowIfNull(module, "module");

            // TODO validate module
            var ps = System.Management.Automation.PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddCommand("Save-Module")
                .AddParameter("Path", tempModulePath)
                .AddParameter("InputObject", module);
            ps.Invoke();
        }

        private void SetupPSModulePath()
        {
            oldPSModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            curPSModulePath = oldPSModulePath + ";" + tempModulePath;
            Environment.SetEnvironmentVariable("PSModulePath", curPSModulePath, EnvironmentVariableTarget.Process);
        }

        private void RestorePSModulePath()
        {
            Environment.SetEnvironmentVariable("PSModulePath", oldPSModulePath, EnvironmentVariableTarget.Process);
        }
        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Creates an instance of the ModuleDependencyHandler class
        /// </summary>
        /// <param name="runspace">Runspace in which the instance runs powershell cmdlets to find and save modules</param>
        /// <param name="moduleRepository">Name of the repository from where to download the modules. By default it is PSGallery. This should be a registered repository. </param>
        /// <param name="tempPath">Path to the user scoped temporary directory</param>
        /// <param name="localAppDataPath">Path to the local app data directory</param>
        public ModuleDependencyHandler(
            Runspace runspace = null,
            string moduleRepository = null,
            string tempPath = null,
            string localAppDataPath = null)
        {
            if (runspace == null)
            {
                Runspace = RunspaceFactory.CreateRunspace();
            }
            else
            {
                Runspace = runspace;
            }
            
            if (Runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
            {
                Runspace.Open();
            }
            else if (Runspace.RunspaceStateInfo.State != RunspaceState.Opened)
            {
                throw new ArgumentException(string.Format(
                        "Runspace state cannot be {0}",
                        runspace.RunspaceStateInfo.State.ToString()));
            }

            // TODO should set PSSA environment variables outside this class
            // Should be set in ScriptAnalyzer class
            // and then passed into modulehandler
            TempPath = tempPath;
            LocalAppDataPath = localAppDataPath;
            PSSAAppDataPath = pssaAppDataPath;
            ModuleRepository = moduleRepository;
                        
            modulesFound = new Dictionary<string, PSObject>();            

            // TODO Add PSSA Version in the path
            symLinkPath = Path.Combine(pssaAppDataPath, symLinkName);
            SetupPSSAAppData();
            SetupPSModulePath();
            
        }

        /// <summary>
        /// Encapsulates Find-Module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>A PSObject if it finds the modules otherwise returns null</returns>
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

        /// <summary>
        /// SaveModule version that doesn't throw
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>True if it can save a module otherwise false.</returns>
        public bool TrySaveModule(string moduleName)
        {
            try
            {
                SaveModule(moduleName);
                return true;
            }
            catch
            {
                // log exception to verbose
                return false;
            }
        }

        /// <summary>
        /// Encapsulates Save-Module cmdlet
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        public void SaveModule(string moduleName)
        {
            ThrowIfNull(moduleName, "moduleName");
            if (IsModulePresent(moduleName))
            {                
                return;
            }
            var ps = System.Management.Automation.PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddCommand("Save-Module")
                .AddParameter("Path", tempModulePath)
                .AddParameter("Name", moduleName)
                .AddParameter("Force");
            ps.Invoke();
        }

        /// <summary>
        /// Get the module name from the error extent
        /// 
        /// If a parser encounters Import-DSCResource -ModuleName SomeModule 
        /// and if SomeModule is not present in any of the PSModulePaths, the
        /// parser throws ModuleNotFoundDuringParse Error. We correlate the 
        /// error message with extent to extract the module name as the error
        /// record doesn't provide direct access to the missing module name.
        /// </summary>
        /// <param name="error">Parse error</param>
        /// <param name="ast">AST of the script that contians the parse error</param>
        /// <returns>The name of the module that caused the parser to throw the error. Returns null if it cannot extract the module name.</returns>
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

        /// <summary>
        /// Disposes the runspace and restores the PSModulePath. 
        /// </summary>
        public void Dispose()
        {
            runspace.Dispose();
            RestorePSModulePath();
        }

        #endregion Public Methods
    }
}