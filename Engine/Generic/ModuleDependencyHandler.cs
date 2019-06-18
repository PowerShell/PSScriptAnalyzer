// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#if !PSV3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

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
                    = string.IsNullOrWhiteSpace(value)
                    ? Path.GetTempPath()
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
#if CORECLR
                localAppdataPath
                    = string.IsNullOrWhiteSpace(value)
                    ? Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LOCALAPPDATA" : "HOME") //Environment.GetEnvironmentVariable("LOCALAPPDATA")
                    : value;
#else
                localAppdataPath
                    = string.IsNullOrWhiteSpace(value)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    : value;
#endif
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
                    = string.IsNullOrWhiteSpace(value)
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
                    if (Directory.Exists(tempModulePath))
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

        private bool IsModulePresentInTempModulePath(string moduleName)
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
            string line; 
            using (var file = File.Open(symLinkPath, FileMode.Open))
            using (var fileStream = new StreamReader(file))
            {
                line = fileStream.ReadLine();
            }
            return line;
        }

        private void SetupPSModulePath()
        {
            oldPSModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            curPSModulePath = oldPSModulePath + Path.PathSeparator + tempModulePath;
#if CORECLR
            Environment.SetEnvironmentVariable("PSModulePath", curPSModulePath);
#else
            Environment.SetEnvironmentVariable("PSModulePath", curPSModulePath, EnvironmentVariableTarget.Process);
#endif
        }

        private void RestorePSModulePath()
        {

#if CORECLR
            Environment.SetEnvironmentVariable("PSModulePath", oldPSModulePath);
#else
            Environment.SetEnvironmentVariable("PSModulePath", oldPSModulePath, EnvironmentVariableTarget.Process);
#endif
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
            Runspace runspace,
            string moduleRepository = null,
            string tempPath = null,
            string localAppDataPath = null)
        {

            ThrowIfNull(runspace, "runspace");
            if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
            {
                throw new ArgumentException(string.Format(
                        "Runspace state cannot be in {0} state. It must be in Opened state",
                        runspace.RunspaceStateInfo.State.ToString()));
            }
            Runspace = runspace;

            // TODO should set PSSA environment variables outside this class
            // Should be set in ScriptAnalyzer class
            // and then passed into modulehandler
            TempPath = tempPath;
            LocalAppDataPath = localAppDataPath;
            ModuleRepository = moduleRepository;
            pssaAppDataPath = Path.Combine(
                LocalAppDataPath,
                string.IsNullOrWhiteSpace(pssaAppDataPath)
                        ? "PSScriptAnalyzer"
                        : pssaAppDataPath);

            modulesFound = new Dictionary<string, PSObject>(StringComparer.OrdinalIgnoreCase);            

            // TODO Add PSSA Version in the path
            symLinkPath = Path.Combine(pssaAppDataPath, symLinkName);
            SetupPSSAAppData();
            SetupPSModulePath();
            
        }

        /// <summary>
        /// SaveModule version that doesn't throw
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="moduleVersion">(Optional) version of the module</param>
        /// <returns>True if it can save a module otherwise false.</returns>
        public bool TrySaveModule(string moduleName, Version moduleVersion)
        {
            try
            {
                SaveModule(moduleName, moduleVersion);
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
        /// <param name="moduleVersion">(Optional) version of the module</param>
        public void SaveModule(string moduleName, Version moduleVersion)
        {
            ThrowIfNull(moduleName, "moduleName");
            if (IsModulePresentInTempModulePath(moduleName))
            {                
                return;
            }
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddCommand("Save-Module")
                    .AddParameter("Path", tempModulePath)
                    .AddParameter("Name", moduleName)
                    .AddParameter("Repository", moduleRepository)
                    .AddParameter("Force");
                if (moduleVersion != null)
                {
                    ps.AddParameter("RequiredVersion", moduleVersion);
                }
                ps.Invoke();
            }
        }

        /// <summary>
        /// Encapsulates Get-Module to check the availability of the module on the system
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="moduleVersion"></param>
        /// <returns>True indicating the presence of the module, otherwise false</returns>
        public bool IsModuleAvailable(string moduleName, Version moduleVersion)
        {
            ThrowIfNull(moduleName, "moduleName");
            IEnumerable<PSModuleInfo> availableModules;
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddCommand("Get-Module")
                    .AddParameter("Name", moduleName)
                    .AddParameter("ListAvailable");
                if (moduleVersion != null)
                {
                    ps.AddCommand("Where-Object")
                      .AddParameter("Filterscript", ScriptBlock.Create($"$_.Version -eq '{moduleVersion}'"));
                }
                availableModules = ps.Invoke<PSModuleInfo>();

            }
            return availableModules != null ? availableModules.Any() : false;
        }


        /// <summary>
        /// Extracts out the module names from the error extent that are not available
        /// 
        /// This handles the following case.
        /// Import-DSCResourceModule -ModuleName ModulePresent,ModuleAbsent
        /// 
        /// ModulePresent is present in PSModulePath whereas ModuleAbsent is not. 
        /// But the error exent coverts the entire extent and hence we need to check
        /// which module is actually not present so as to be downloaded
        /// </summary>
        /// <param name="error"></param>
        /// <param name="ast"></param>
        /// <param name="moduleVersion"></param>
        /// <returns>An enumeration over the module names that are not available</returns>
        public IEnumerable<string> GetUnavailableModuleNameFromErrorExtent(ParseError error, ScriptBlockAst ast, out Version moduleVersion)
        {
            ThrowIfNull(error, "error");
            ThrowIfNull(ast, "ast");
            var moduleNames = ModuleDependencyHandler.GetModuleNameFromErrorExtent(error, ast, out moduleVersion);
            if (moduleNames == null)
            {
                return null;
            }
            var unavailableModules = new List<string>();
            foreach (var moduleName in moduleNames)
            {
                if (!IsModuleAvailable(moduleName, moduleVersion))
                {
                    unavailableModules.Add(moduleName);
                }
            }

            return unavailableModules;
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
        /// <param name="moduleVersion">Specifc version of the required module</param>
        /// <returns>The name of the module that caused the parser to throw the error. Returns null if it cannot extract the module name.</returns>
        public static IEnumerable<string> GetModuleNameFromErrorExtent(ParseError error, ScriptBlockAst ast, out Version moduleVersion)
        {
            moduleVersion = null;
            ThrowIfNull(error, "error");
            ThrowIfNull(ast, "ast");
            var statement = ast.Find(x => x.Extent.Equals(error.Extent), true);
            var dynamicKywdAst = statement as DynamicKeywordStatementAst;
                if (dynamicKywdAst == null)
            {
                return null;
            }
            // check if the command name is import-dscmodule
            // right now we handle only the following forms
            // 1. Import-DSCResourceModule -ModuleName somemodule
            // 2. Import-DSCResourceModule -ModuleName somemodule1 -ModuleVersion major.minor.patch.build
            // 3. Import-DSCResourceModule -ModuleName somemodule1,somemodule2
            var dscKeywordAst = dynamicKywdAst.CommandElements[0] as StringConstantExpressionAst;
            if (dscKeywordAst == null || !dscKeywordAst.Value.Equals("Import-DscResource", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // find a parameter named modulename
            int positionOfModuleNameParamter = 0;
            int positionOfModuleVersionParameter = 0;
            for (int i = 1; i < dynamicKywdAst.CommandElements.Count; i++)
            {
                var paramAst = dynamicKywdAst.CommandElements[i] as CommandParameterAst;
                // TODO match the initial letters only
                if (paramAst != null && paramAst.ParameterName.Equals("ModuleName", StringComparison.OrdinalIgnoreCase))
                {
                    if (i == dynamicKywdAst.CommandElements.Count)
                    {
                        // command was Save-DscDependency ... -ModuleName -> module name missing
                        return null;
                    }
                    positionOfModuleNameParamter = i + 1;
                    continue;
                }

                if (paramAst != null && paramAst.ParameterName.Equals("ModuleVersion", StringComparison.OrdinalIgnoreCase))
                {
                    if (i == dynamicKywdAst.CommandElements.Count)
                    {
                        // command was Save-DscDependency ... -ModuleVersion -> module version missing
                        return null;
                    }
                    positionOfModuleVersionParameter = i + 1;
                    continue;
                }
            }
            
            var modules = new List<string>();
            
            var paramValAst = dynamicKywdAst.CommandElements[positionOfModuleNameParamter];

            // import-dscresource -ModuleName module1
            if (paramValAst is StringConstantExpressionAst paramValStrConstExprAst)
            {                
                modules.Add(paramValStrConstExprAst.Value);

                // import-dscresource -ModuleName module1 -ModuleVersion major.minor.patch.build
                var versionParameterAst = dynamicKywdAst.CommandElements[positionOfModuleVersionParameter] as StringConstantExpressionAst;
                if (versionParameterAst != null)
                {
                    Version.TryParse(versionParameterAst.Value, out moduleVersion); // ignore return value since a module version of null means no version
                }
                return modules;
            }

            // Import-DscResource –ModuleName @{ModuleName="module1";ModuleVersion="1.2.3.4"}
            //var paramValAstHashtableAst = paramValAst.Find(oneAst => oneAst is HashtableAst, true) as HashtableAst;
            if (paramValAst.Find(oneAst => oneAst is HashtableAst, true) is HashtableAst paramValAstHashtableAst)
            {
                var moduleNameTuple = paramValAstHashtableAst.KeyValuePairs.SingleOrDefault(x => x.Item1.Extent.Text.Equals("ModuleName"));
                var moduleName = moduleNameTuple.Item2.Find(astt => astt is StringConstantExpressionAst, true) as StringConstantExpressionAst;
                if (moduleName == null)
                {
                    return null;
                }
                modules.Add(moduleName.Value);
                var moduleVersionTuple = paramValAstHashtableAst.KeyValuePairs.SingleOrDefault(x => x.Item1.Extent.Text.Equals("ModuleVersion"));
                var moduleVersionAst = moduleVersionTuple.Item2.Find(astt => astt is StringConstantExpressionAst, true) as StringConstantExpressionAst;
                Version.TryParse(moduleVersionAst.Value, out moduleVersion);
                return modules;
            }

            // import-dscresource -ModuleName module1,module2
            if (paramValAst is ArrayLiteralAst paramValArrLtrlAst)
            {
                foreach (var elem in paramValArrLtrlAst.Elements)
                {
                    var elemStrConstExprAst = elem as StringConstantExpressionAst;
                    if (elemStrConstExprAst != null)
                    {
                        modules.Add(elemStrConstExprAst.Value);
                    }
                }
                if (modules.Count == 0)
                {
                    return null;
                }
                return modules;
            }

            return null;
        }

        /// <summary>
        /// Disposes the runspace and restores the PSModulePath. 
        /// </summary>
        public void Dispose()
        {
            RestorePSModulePath();
        }

#endregion Public Methods
    }
}
#endif // !PSV3