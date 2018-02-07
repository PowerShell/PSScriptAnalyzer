// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// A class to check if a script uses types compatible with a given OS, PowerShell edition, and PowerShell version.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    public class UseCompatibleTypes : IScriptRule
    {
        // Valid parameters for this rule.
        private readonly string validParameters;

        // Path of script being analyzed by ScriptAnalyzer.
        private string scriptPath;

        // Lists each target platform (broken down into PowerShell edition, version, os).
        private Dictionary<string, dynamic> platformSpecMap;

        // List of full type names for each target platform.
        private Dictionary<string, HashSet<string>> psTypeMap;

        // List of full type names from desktop PowerShell (used a reference to find type names).
        private HashSet<string> referenceMap;

        // Beginning of name of PowerShell desktop version reference file.
        private readonly string referenceFileName = "desktop-5.1*";

        // List of type accelerator names and their corresponding full names.
        private Dictionary<string, string> typeAcceleratorMap;

        // Name of the type accelerator file.
        private readonly string typeAccFileName = "typeAccelerators.json";

        // List of .Net namespaces (first word only).
        private List<string> knownNamespaces = new List<string> { "System.", "Microsoft.", "Newtonsoft.", "Internal." };

        // List of all TypeAst objects (TypeConstraintAst, TypeExpressionAst, and 
        // types used with 'New-Object') found in ast.
        private List<dynamic> allTypesFromAst;

        // List of user created types found in ast (TypeDefinitionAst).
        private List<string> customTypes;

        // List of all diagnostic records for incompatible types.
        private List<DiagnosticRecord> diagnosticRecords;

        private bool IsInitialized;
        private bool hasInitializationError;

        private class fullTypeNameObject
        {
            public string fullName;
            public bool isCustomType;
            public bool isTypeAccelerator;

            public fullTypeNameObject(bool customType)
            {
                fullName = null;
                isCustomType = customType;
                isTypeAccelerator = false;
            }
        }

        public UseCompatibleTypes()
        {
            validParameters = "compatibility";
            IsInitialized = false;
        }

        /// <summary>
        /// Analyzes the given ast to find the violation(s).
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null.</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>An enumerable type containing the violations.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            // We do not want to initialize the data structures if the rule is not being used for analysis,
            // hence we initialize when this method is called for the first time.
            if (!IsInitialized)
            {
                Initialize();
            }

            if (hasInitializationError)
            {
                Console.WriteLine("There was an error running the UseCompatibleTypes Rule. Please check the error log in the Settings file for more info.");
                return new DiagnosticRecord[0];
            }

            diagnosticRecords.Clear();

            scriptPath = fileName;
            allTypesFromAst = new List<dynamic>();
            customTypes = new List<string>();

            // TypeConstraintAsts  
            IEnumerable<Ast> constraintAsts = ast.FindAll(testAst => testAst is TypeConstraintAst, true);
            addAstElementToList(constraintAsts);

            // TypeExpressionAsts 
            IEnumerable<Ast> expressionAsts = ast.FindAll(testAst => testAst is TypeExpressionAst, true);
            addAstElementToList(expressionAsts);

            // Types named within a command.  We want only types used with the 'New-Object'
            // cmdlet, but will filter those out later.
            IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);
            addAstElementToList(commandAsts);

            // Types declared by the user (defined within a user-created class).
            IEnumerable<Ast> definitionAsts = ast.FindAll(testAst => testAst is TypeDefinitionAst, true);
            foreach (Ast item in definitionAsts)
            {
                string customType = item.GetType().GetProperty("Name").GetValue(item).ToString();
                customTypes.Add(customType);
            }

            // These are namespaces used in the script.  This will help getting full type names for types
            // from ast objects that only give us the type name as a string.  We will add these to the 
            // beginning of our known namespaces list so we check those first.
            IEnumerable<Ast> useStatementAsts = ast.FindAll(testAst => testAst is UsingStatementAst, true);
            foreach (Ast item in useStatementAsts)
            {
                string nspace = item.GetType().GetProperty("Name").GetValue(item).ToString();
                knownNamespaces.Insert(0, nspace + ".");
            }

            // If we have no types to check, we can exit from this rule.
            if (allTypesFromAst.Count == 0)
            {
                return new DiagnosticRecord[0];
            }

            CheckCompatibility();

            return diagnosticRecords;
        }

        /// <summary>
        /// Check if type is present in the target platform type list.
        /// If not, create a Diagnostic Record for that type.
        /// </summary>
        private void CheckCompatibility()
        {
            foreach (dynamic typeObject in allTypesFromAst)
            {
                List<fullTypeNameObject> fullTypeNameObjectList = RetrieveFullTypeName(typeObject);

                foreach (fullTypeNameObject nameObject in fullTypeNameObjectList)
                {
                    int couldNotResolveCount = 0;

                    foreach (dynamic platform in psTypeMap)
                    {
                        if (nameObject.isCustomType)
                        {
                            // If this is a custom type, try and find it in our definition ast list.  If it's there
                            // we can ignore it.
                            if (customTypes.Contains(nameObject.fullName, StringComparer.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            // If this is not a known custom type, it could mean it is a type the user created with 
                            // Add-Type or with a string variable containing the type information.  There is no reasonable
                            // way to check for that, so create a diagnostic record.
                            else
                            {
                                // If we have multiple target platforms to check and we cannot resolve a type, we don't want
                                // multiple 'could not resolve' errors for the same type.
                                if (couldNotResolveCount < 1)
                                {
                                    GenerateDiagnosticRecord(typeObject, nameObject.fullName, platform.Key, false, nameObject.isTypeAccelerator);
                                }
                                couldNotResolveCount++;
                            }
                        }
                        else
                        {
                            // Does the target platform library contain this type?           
                            if (platform.Value.Contains(nameObject.fullName))
                            {
                                continue;
                            }
                            else
                            {
                                // If not, then the type is incompatible so generate an error Diagnostic Record.
                                GenerateDiagnosticRecord(typeObject, nameObject.fullName, platform.Key, true, nameObject.isTypeAccelerator);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create an instance of DiagnosticRecord and add it to diagnosticRecords list.
        /// </summary>
        private void GenerateDiagnosticRecord(
            dynamic astObject,
            string fullTypeName,
            string platform,
            bool resolved,
            bool typeAccelerator)
        {
            var extent = astObject.Extent;
            var platformInfo = platformSpecMap[platform];

            if (resolved)
            {
                // Here we are just including the type accelerator name so the type will be easier to spot in the script
                // on the line number we provide in their diagnostic record.
                string accelerator = "";
                if (typeAccelerator)
                {
                    try
                    {
                        accelerator = " (" + astObject.TypeName?.ToString() + ")";
                    }
                    catch { }
                }

                // Diagnostic Record for resolved types that do not exist on 
                // target platform.
                diagnosticRecords.Add(new DiagnosticRecord(
                String.Format(
                    Strings.UseCompatibleTypesError,
                    fullTypeName + accelerator,
                    platformInfo.PSEdition,
                    platformInfo.PSVersion,
                    platformInfo.OS),
                extent,
                GetName(),
                GetDiagnosticSeverity(),
                scriptPath,
                null,
                null));
            }
            else
            {
                // Diagnostic Record for unresolved (usually user-created) types.
                diagnosticRecords.Add(new DiagnosticRecord(
                    String.Format(
                        Strings.UseCompatibleTypesUnresolvedError,
                        fullTypeName
                        ),
                    extent,
                    GetName(),
                    GetDiagnosticSeverity(),
                    scriptPath,
                    null,
                    null));
            }
        }

        /// <summary>
        /// Initialize data structures needed to check type compatibility.
        /// </summary>
        private void Initialize()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            psTypeMap = new Dictionary<string, HashSet<string>>((StringComparer.OrdinalIgnoreCase));
            referenceMap = new HashSet<string>((StringComparer.OrdinalIgnoreCase));
            typeAcceleratorMap = new Dictionary<string, string>();
            platformSpecMap = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            SetupTypesDictionary();
            IsInitialized = true;
        }

        /// <summary>
        /// Sets up dictionaries indexed by PowerShell version/edition and OS; and
        /// sets up type accelerator dictionary.
        /// </summary>
        private void SetupTypesDictionary()
        {
            // If the method encounters any error it returns early, which implies there is an initialization error.
            // The error will be written to the log file in the Settings directory.
            hasInitializationError = true;

            // Get path to Settings Directory (where the json dictionaries are located).
            string settingsPath = Settings.GetShippedSettingsDirectory();

            if (String.IsNullOrEmpty(settingsPath))
            {
                return;
            }

            string logFile = CreateLogFileName(settingsPath);

            // Retrieve rule parameters provided by user.
            Dictionary<string, object> ruleParams = Helper.Instance.GetRuleArguments(GetName());

            // If there are no params or if none are 'compatibility', return.
            if (ruleParams == null || !RuleParamsValid(ruleParams))
            {
                WriteToLogFile(logFile, "Parameters for UseCompatibleTypes are invalid.  Make sure to include a 'compatibility' param in your Settings file.");
                return;
            }

            // For each target listed in the 'compatibility' param, add it to compatibilityList.
            string[] compatibilityArray = ruleParams["compatibility"] as string[];

            if (compatibilityArray == null || compatibilityArray.Length.Equals(0))
            {
                WriteToLogFile(logFile, "Compatibility parameter is empty.");
                return;
            }

            List<string> compatibilityList = new List<string>();

            foreach (string target in compatibilityArray)
            {
                if (String.IsNullOrEmpty(target))
                {
                    // ignore invalid entries
                    continue;
                }
                compatibilityList.Add(target);
            }

            if (compatibilityList.Count.Equals(0))
            {
                WriteToLogFile(logFile, "There are no target platforms listed in the compatibility parameter.");
                return;
            }

            // Create our platformSpecMap from the target platforms in the compatibilityList.
            foreach (string target in compatibilityList)
            {
                string psedition, psversion, os;

                // ignore invalid entries
                if (GetVersionInfoFromPlatformString(target, out psedition, out psversion, out os))
                {
                    platformSpecMap.Add(target, new { PSEdition = psedition, PSVersion = psversion, OS = os });
                }
            }

            // Find corresponding dictionaries for target platforms and create type maps.
            ProcessDirectory(settingsPath, compatibilityList);

            if (psTypeMap.Keys.Count != compatibilityList.Count())
            {
                WriteToLogFile(logFile, "One or more of the target platforms listed in the compatibility parameter is not valid.");
                return;
            }

            // Set up the reference type map (from desktop PowerShell).
            referenceMap = SetUpAdditionalTypeMap(settingsPath, referenceFileName);

            // Set up type accelerator map.
            CreateTypeAcceleratorMap(settingsPath);

            // Reached this point, so no initialization error.
            hasInitializationError = false;
        }

        /// <summary>
        /// Search the Settings directory for files in the form [PSEdition]-[PSVersion]-[OS].json.
        /// For each json file found that matches our target platforms, parse file to create type map.
        /// </summary>
        private void ProcessDirectory(string path, List<string> compatibilityList)
        {
            var jsonFiles = Directory.EnumerateFiles(path, "*.json");
            if (jsonFiles == null)
            {
                return;
            }

            foreach (string file in jsonFiles)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                if (!compatibilityList.Contains(fileNameWithoutExt, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                dynamic deserialized = JObject.Parse(File.ReadAllText(file));
                psTypeMap[fileNameWithoutExt] = GetTypesFromData(deserialized);
            }
        }

        /// <summary>
        /// Get a hashset of full type names from a deserialized json file.
        /// </summary>
        private HashSet<string> GetTypesFromData(dynamic deserializedObject)
        {
            HashSet<string> types = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            dynamic typeList = deserializedObject.Types;
            foreach (dynamic type in typeList)
            {
                string name = type.Name.ToString();
                string nameSpace = type.Namespace.ToString();
                string fullName = nameSpace + "." + name;
                types.Add(fullName);
            }
            return types;
        }

        /// <summary>
        /// Create the type accelerator map from json file in Settings directory.
        /// </summary>
        private void CreateTypeAcceleratorMap(string path)
        {
            string typeAccFile = Path.Combine(path, typeAccFileName);
            dynamic deserialized = JObject.Parse(File.ReadAllText(typeAccFile));

            foreach (dynamic typeAcc in deserialized)
            {
                typeAcceleratorMap.Add(typeAcc.Name.ToString(), typeAcc.Value.ToString());
            }
        }

        /// <summary>
        /// Set up type map from the latest desktop version of PowerShell.
        /// </summary>
        private HashSet<string> SetUpAdditionalTypeMap(string path, string fileName)
        {
            string[] typeFile = Directory.GetFiles(path, fileName);
            dynamic deserialized = JObject.Parse(File.ReadAllText(typeFile[0]));
            return GetTypesFromData(deserialized);
        }

        ///<summary>
        /// Get full type name from ast object.
        ///</summary>
        private List<fullTypeNameObject> RetrieveFullTypeName(dynamic typeObject)
        {
            // Check to see if our object is a CommandAst. 
            string AstType = (typeObject.GetType()).Name;

            if (String.Equals(AstType, "CommandAst", StringComparison.OrdinalIgnoreCase))
            {
                return GetFullNameFromNewObjectCommand(typeObject);
            }

            // If we make it here our ast object is either a TypeConstraintAst or a TypeExpressionAst.

            List<fullTypeNameObject> fullNameObjectList = new List<fullTypeNameObject>();
            var typeNameProperty = typeObject.TypeName;

            var type = typeNameProperty.GetReflectionType();

            if (type != null)
            {
                string[] splitNames = type.ToString().Split(new Char[] { '[', ',', ']' },
                                                           StringSplitOptions.RemoveEmptyEntries);

                foreach (string name in splitNames)
                {
                    fullTypeNameObject fullTypeNameInfoObject = new fullTypeNameObject(false);
                    fullTypeNameInfoObject.fullName = name;
                    fullNameObjectList.Add(fullTypeNameInfoObject);
                }
            }
            else // Reflection couldn't give us the type names.
            {
                string[] splitNames = typeNameProperty.ToString().Split(new Char[] { '[', ',', ']' },
                                                           StringSplitOptions.RemoveEmptyEntries);

                // If splitNames length is greater than 1, then we want to try and use the ast object again to 
                // get full type names.  This is because we have an instance of a type taking parameters and we will not
                // get the full type name from the string only. 
                // Example:  the string 'System.Collections.Generic.List[string]' will not give us 'System.Collections.Generic.List`1'
                // which is what we want.

                if (splitNames.Length > 1)
                {
                    fullTypeNameObject outsideType = new fullTypeNameObject(false);
                    var typeName = typeNameProperty.TypeName;

                    outsideType.fullName = GetAstField(typeName, "_type");

                    if (outsideType.fullName == null)
                    {
                        outsideType.fullName = GetAstField(typeName, "_name");
                    }
                    // Sometimes the full name of the outsideType will still contain '[]' so we need to remove it.
                    string[] split = outsideType.fullName.Split('[');
                    outsideType.fullName = split[0];

                    // If this is already a full type name we can add it to our list.
                    if (StartsWithKnownNamespace(outsideType.fullName))
                    {
                        fullNameObjectList.Add(outsideType);
                    }
                    // If not, we have to find it ourselves.
                    else
                    {
                        outsideType = TryToBuildFullTypeName(outsideType);
                        fullNameObjectList.Add(outsideType);
                    }

                    // Our inside types are the GenericArguments on our typeNameProperty object.
                    dynamic insideTypes = typeNameProperty.GenericArguments;

                    foreach (dynamic argument in insideTypes)
                    {
                        fullTypeNameObject insideType = new fullTypeNameObject(false);
                        insideType.fullName = GetAstField(argument, "_type");

                        if (insideType.fullName == null)
                        {
                            insideType.fullName = GetAstField(argument, "_name");
                        }

                        // Now we'll do the same check as we did for the outside type.
                        if (StartsWithKnownNamespace(insideType.fullName))
                        {
                            fullNameObjectList.Add(insideType);
                        }
                        else
                        {
                            insideType = TryToBuildFullTypeName(insideType);
                            fullNameObjectList.Add(insideType);
                        }
                    }
                }
                // SplitNames has only one element.
                else
                {
                    fullTypeNameObject fullTypeNameInfoObject = new fullTypeNameObject(false);
                    fullTypeNameInfoObject.fullName = splitNames[0];

                    if (StartsWithKnownNamespace(fullTypeNameInfoObject.fullName))
                    {
                        fullNameObjectList.Add(fullTypeNameInfoObject);
                    }
                    else
                    {
                        fullTypeNameInfoObject = TryToBuildFullTypeName(fullTypeNameInfoObject);
                        fullNameObjectList.Add(fullTypeNameInfoObject);
                    }
                }
            }
            return fullNameObjectList;
        }

        /// <summary>
        /// Retrieve full type names from a 'New-Object' CommandAst.
        /// </summary>
        private List<fullTypeNameObject> GetFullNameFromNewObjectCommand(dynamic typeObject)
        {
            // Each commandAst object has the CommandElements property that looks like the following:
            // 
            // CommandElements[0] = The name of the command i.e. 'Get-Command', 'New-Object'.
            //
            // CommandElements[1] = Either a parameter name OR the object/type.
            //
            // CommandElements[2] = If CommandElements[1] is a parameter, then this is the object/type 
            //                      i.e.'string', 'myType'.                 

            List<fullTypeNameObject> fullNameObjectList = new List<fullTypeNameObject>();
            string typeName = null;

            try
            {
                // Get only the 'New-Object' commandAsts.
                if (String.Equals(typeObject.GetCommandName(), "New-Object", StringComparison.OrdinalIgnoreCase))
                {
                    StringConstantExpressionAst typeBeingCreated = null;

                    try
                    {
                        string possibleParam = (typeObject.CommandElements[1].GetType()).Name;

                        // Is there a named parameter?
                        if (String.Equals(possibleParam, "CommandParameterAst", StringComparison.OrdinalIgnoreCase))
                        {
                            // Now we only want the parameters that include 'type'.
                            string paramName = typeObject.CommandElements[1].ParameterName.ToLower();

                            if (paramName.Contains("type"))
                            {
                                typeBeingCreated = typeObject.CommandElements[2] as StringConstantExpressionAst;
                            }
                        }
                        // If the secondElement is not a parameter name, then it will be the type being created.
                        else
                        {
                            typeBeingCreated = typeObject.CommandElements[1] as StringConstantExpressionAst;
                        }
                    }
                    catch { }

                    // There is a possibility typeBeingCreated could be an array (string[]) in which case we 
                    // just want 'string', or more than one type (someType[string, int]) in which case we want 
                    // all three types.
                    string[] typeNameComponents = typeBeingCreated.Value.Split(new Char[] { '[', ',', ']', ' ', '\'', '(', ')' },
                                                        StringSplitOptions.RemoveEmptyEntries);

                    // For full type names that specify the number of parameters they take (System.Collections.Generic.SortedList`2), 
                    // we will very rarely get the full type name with " `2 " in it from the string after 'New-Object'.
                    // Most likely the string would be along the lines of: System.Collections.Generic.SortedList[string, string].
                    // If there is more than one item in our typeNameComponents array we know what number to put after the ` based 
                    // on how many items we have in typeNameComponents.
                    if (typeNameComponents.Length > 1 && (!typeNameComponents[0].Contains("`")))
                    {
                        string parameterNumber = (typeNameComponents.Length - 1).ToString();
                        typeNameComponents[0] = typeNameComponents[0] + "`" + parameterNumber;
                    }

                    // Now we need to find the full name for each of our types.
                    foreach (string name in typeNameComponents)
                    {
                        fullTypeNameObject fullNameObject = new fullTypeNameObject(false);
                        fullNameObject.fullName = name;

                        // Is our name already a full name (includes namespace)?
                        if (StartsWithKnownNamespace(name))
                        {
                            fullNameObjectList.Add(fullNameObject);
                        }
                        else
                        {
                            fullNameObject = TryToBuildFullTypeName(fullNameObject);
                            fullNameObjectList.Add(fullNameObject);
                        }
                    }
                }
            }
            // If the CommandAst is a type we don't want to analyze (like a scriptblock), or
            // if the New-Object command is tyring to create a ComObject, the properties we are
            // trying to access in the above 'if' statement won't exist and will throw an exception.
            // We'll just catch it and move on since we don't want to deal with those anyway.
            catch (System.Exception) { }

            return fullNameObjectList;
        }

        ///<summary>
        /// Retrieves a non-public field from an ast object.
        ///</summary>
        private string GetAstField(dynamic typeNamePropertyObject, string desiredProperty)
        {
            dynamic property = typeNamePropertyObject.GetType()?.GetField(desiredProperty,
                               BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(
                               typeNamePropertyObject);

            if (desiredProperty == "_type" && property != null)
            {
                return property.FullName.ToString();
            }
            else if (property != null)
            {
                return property.ToString();
            }
            else
            {
                return property;
            }
        }

        ///<summary>
        /// Tries to get a full type name from our built-in maps and libraries.
        ///</summary>
        private fullTypeNameObject TryToBuildFullTypeName(fullTypeNameObject typeObject)
        {
            string name = typeObject.fullName;

            // Is this a type accelerator? 
            typeObject.fullName = CheckTypeAcceleratorMap(name);

            if (typeObject.fullName != null)
            {
                typeObject.isTypeAccelerator = true;
            }
            else
            {
                // Can we create a full type name by checking known namespaces?
                typeObject.fullName = TryKnownNameSpaces(name);

                if (typeObject.fullName == null)
                {
                    typeObject.fullName = name;
                    // Is this a UWP type?
                    if (!PossibleUWPtype(typeObject.fullName))
                    {
                        // We will assume this is a user-created type.
                        typeObject.isCustomType = true;
                    }
                }
            }
            return typeObject;
        }

        ///<summary>
        /// Check Type Accelerator map for full type name.
        ///</summary>
        private string CheckTypeAcceleratorMap(string typeName)
        {
            typeName = typeName.ToLower();
            string value = null;
            if (typeAcceleratorMap.TryGetValue(typeName, out value))
            {
                return value;
            }
            return value;
        }

        ///<summary>
        /// Look up type in all target platforms and reference map by adding known namespaces
        /// to the beginning. If the script has any 'using Statements' we will check those namespaces
        /// first. (We will take the first match we find regardless of platform because
        /// all we want is the full type name).
        ///</summary>
        private string TryKnownNameSpaces(string TypeName)
        {
            foreach (string nspace in knownNamespaces)
            {
                string possibleFullName = nspace + TypeName;

                // Try our target platforms first.
                foreach (var platform in psTypeMap)
                {
                    if (platform.Value.Contains(possibleFullName, StringComparer.OrdinalIgnoreCase))
                    {
                        return possibleFullName;
                    }
                }

                // If no match in our target platforms, try our reference map.
                if (referenceMap.Contains(possibleFullName, StringComparer.OrdinalIgnoreCase))
                {
                    return possibleFullName;
                }
            }
            // No match anywhere.
            return null;
        }

        ///<summary>
        /// Check if type name starts with a known namespace.
        ///</summary>
        private bool StartsWithKnownNamespace(string typeName)
        {
            if (typeName != null)
            {
                foreach (string nspace in knownNamespaces)
                {
                    if (typeName.StartsWith(nspace, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        ///<summary>
        /// Check if type is from a Universal Windows Platform (UWP) namespace. If the typeName 
        /// starts with 'Windows' it does not ALWAYS mean it's from a UWP namespace, but usually it is.  
        /// We are using this check to prevent our type from being labeled 'custom' and therefore giving
        /// the incorrect error message.
        ///</summary>
        private bool PossibleUWPtype(string typeName)
        {
            if (typeName.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the found ast objects to the 'master' type list (allTypesFromAst).
        /// </summary>
        private void addAstElementToList(dynamic astList)
        {
            foreach (dynamic foundAst in astList)
            {
                allTypesFromAst.Add(foundAst);
            }
        }

        /// <summary>
        /// Check if rule parameters are valid (at least one parameter must be 'compatibility').
        /// </summary>
        private bool RuleParamsValid(Dictionary<string, object> ruleParams)
        {
            return ruleParams.Keys.Any(key => validParameters.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Create a string with current date/time for log file name.
        /// </summary>
        private string CreateLogFileName(string settingsPath)
        {
            string dateString = String.Format("{0:g}", DateTime.Now);
            string editedDate = (new Regex("\\W")).Replace(dateString, "_");
            string logFile = settingsPath + "\\UseCompatibleTypesErrorLog" + editedDate + ".txt";
            return logFile;
        }

        /// <summary>
        /// Writes an error message to the error log file.
        /// </summary>
        private void WriteToLogFile(string logFile, string message)
        {
            using (StreamWriter writer = File.AppendText(logFile))
            {
                writer.WriteLine(message);
            }
        }

        /// <summary>
        /// Gets PowerShell Edition, Version, and OS from input string.
        /// </summary>
        /// <returns>True if it can retrieve information from string, otherwise, False</returns>
        private bool GetVersionInfoFromPlatformString(
            string fileName,
            out string psedition,
            out string psversion,
            out string os)
        {
            psedition = null;
            psversion = null;
            os = null;
            const string pattern = @"^(?<psedition>core.*|desktop)-(?<psversion>[\S]+)-(?<os>windows|linux|osx|nano|iot)$";
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
            if (match == Match.Empty)
            {
                return false;
            }
            psedition = match.Groups["psedition"].Value;
            psversion = match.Groups["psversion"].Value;
            os = match.Groups["os"].Value;
            return true;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseCompatibleTypesName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning, or information.
        /// </summary>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Error;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Error;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed, or Module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}







