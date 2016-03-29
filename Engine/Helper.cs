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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{

    /// <summary>
    /// This Helper class contains utility/helper functions for classes in ScriptAnalyzer.
    /// </summary>
    public class Helper
    {
        #region Private members

        private CommandInvocationIntrinsics invokeCommand;
        private IOutputWriter outputWriter;
        private Object getCommandLock = new object();

        #endregion

        #region Singleton
        private static object syncRoot = new Object();

        private static Helper instance;

        /// <summary>
        /// The helper instance that handles utility functions
        /// </summary>
        public static Helper Instance
        {
            get
            {
                if (instance == null)
                {
                    Instance = new Helper();
                }

                return instance;
            }
            internal set
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = value;
                    }
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Dictionary contains mapping of cmdlet to alias
        /// </summary>
        private Dictionary<String, List<String>> CmdletToAliasDictionary;

        /// <summary>
        /// Dictionary contains mapping of alias to cmdlet
        /// </summary>
        private Dictionary<String, String> AliasToCmdletDictionary;

        internal TupleComparer tupleComparer = new TupleComparer();

        /// <summary>
        /// My Tokens
        /// </summary>
        public Token[] Tokens { get; set; }

        /// <summary>
        /// Key of the dictionary is keyword or command like configuration or workflows.
        /// Value is a list of integer (in pairs). The first item in a pair is
        /// the starting position of the open curly brace and the second item
        /// is the closing position of the closing curly brace.
        /// </summary>
        private Dictionary<String, List<Tuple<int, int>>> KeywordBlockDictionary;

        /// <summary>
        /// Key of dictionary is ast, value is the corresponding variableanalysis
        /// </summary>
        private Dictionary<Ast, VariableAnalysis> VariableAnalysisDictionary;

        private string[] functionScopes = new string[] { "global:", "local:", "script:", "private:"};

        private string[] variableScopes = new string[] { "global:", "local:", "script:", "private:", "variable:", ":"};

        #endregion

        /// <summary>
        /// Initializes the Helper class.
        /// </summary>
        private Helper()
        {
        }

        /// <summary>
        /// Initializes the Helper class.
        /// </summary>
        /// <param name="invokeCommand">
        /// A CommandInvocationIntrinsics instance for use in gathering 
        /// information about available commands and aliases.
        /// </param>
        /// <param name="outputWriter">
        /// An IOutputWriter instance for use in writing output
        /// to the PowerShell environment.
        /// </param>
        public Helper(
            CommandInvocationIntrinsics invokeCommand,
            IOutputWriter outputWriter)
        {
            this.invokeCommand = invokeCommand;
            this.outputWriter = outputWriter;
        }

        #region Methods
        /// <summary>
        /// Initialize : Initializes dictionary of alias.
        /// </summary>
        public void Initialize()
        {
            CmdletToAliasDictionary = new Dictionary<String, List<String>>(StringComparer.OrdinalIgnoreCase);
            AliasToCmdletDictionary = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
            KeywordBlockDictionary = new Dictionary<String, List<Tuple<int, int>>>(StringComparer.OrdinalIgnoreCase);
            VariableAnalysisDictionary = new Dictionary<Ast, VariableAnalysis>();

            IEnumerable<CommandInfo> aliases = this.invokeCommand.GetCommands("*", CommandTypes.Alias, true);

            foreach (AliasInfo aliasInfo in aliases)
            {
                if (!CmdletToAliasDictionary.ContainsKey(aliasInfo.Definition))
                {
                    CmdletToAliasDictionary.Add(aliasInfo.Definition, new List<String>() { aliasInfo.Name });
                }
                else
                {
                    CmdletToAliasDictionary[aliasInfo.Definition].Add(aliasInfo.Name);
                }

                AliasToCmdletDictionary.Add(aliasInfo.Name, aliasInfo.Definition);
            }
        }

        /// <summary>
        /// Given a cmdlet, return the list of all the aliases.
        /// Also include the original name in the list.
        /// </summary>
        /// <param name="Cmdlet">Name of the cmdlet</param>
        /// <returns></returns>
        public List<String> CmdletNameAndAliases(String Cmdlet)
        {
            List<String> results = new List<String>();
            results.Add(Cmdlet);

            if (CmdletToAliasDictionary.ContainsKey(Cmdlet))
            {
                results.AddRange(CmdletToAliasDictionary[Cmdlet]);
            }

            return results;
        }

        /// <summary>
        /// Given an alias, returns the cmdlet.
        /// </summary>
        /// <param name="Alias"></param>
        /// <returns></returns>
        public string GetCmdletNameFromAlias(String Alias)
        {
            if (AliasToCmdletDictionary.ContainsKey(Alias))
            {
                return AliasToCmdletDictionary[Alias];
            }

            return String.Empty;
        }

        /// <summary>
        /// Given a file path, checks whether the file is part of a dsc resource module
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsDscResourceModule(string filePath)
        {
            DirectoryInfo dscResourceParent = Directory.GetParent(filePath);
            if (null != dscResourceParent)
            {
                DirectoryInfo dscResourcesFolder = Directory.GetParent(dscResourceParent.ToString());
                if (null != dscResourcesFolder)
                {
                    if (String.Equals(dscResourcesFolder.Name, "dscresources", StringComparison.OrdinalIgnoreCase))
                    {
                        // Step 2: Ensure there is a Schema.mof in the same folder as the artifact
                        string schemaMofParentFolder = Directory.GetParent(filePath).ToString();
                        string[] schemaMofFile = Directory.GetFiles(schemaMofParentFolder, "*.schema.mof");

                        // Ensure Schema file exists and is the only one in the DSCResource folder
                        if (schemaMofFile != null && schemaMofFile.Count() == 1)
                        {
                            // Run DSC Rules only on module that matches the schema.mof file name without extension
                            if (String.Equals(schemaMofFile[0].Replace("schema.mof", "psm1"), filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the list of exported function by analyzing the ast
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public HashSet<string> GetExportedFunction(Ast ast)
        {
            HashSet<string> exportedFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> exportFunctionsCmdlet = Helper.Instance.CmdletNameAndAliases("export-modulemember");

            // find functions exported
            IEnumerable<Ast> cmdAsts = ast.FindAll(item => item is CommandAst
                && exportFunctionsCmdlet.Contains((item as CommandAst).GetCommandName(), StringComparer.OrdinalIgnoreCase), true);

            CommandInfo exportMM = Helper.Instance.GetCommandInfo("export-modulemember", CommandTypes.Cmdlet);

            // switch parameters
            IEnumerable<ParameterMetadata> switchParams = (exportMM != null) ? exportMM.Parameters.Values.Where<ParameterMetadata>(pm => pm.SwitchParameter) : Enumerable.Empty<ParameterMetadata>();

            if (exportMM == null)
            {
                return exportedFunctions;
            }

            foreach (CommandAst cmdAst in cmdAsts)
            {
                if (cmdAst.CommandElements == null || cmdAst.CommandElements.Count < 2)
                {
                    continue;
                }

                int i = 1;

                while (i < cmdAst.CommandElements.Count)
                {
                    CommandElementAst ceAst = cmdAst.CommandElements[i];
                    ExpressionAst exprAst = null;

                    if (ceAst is CommandParameterAst)
                    {
                        var paramAst = ceAst as CommandParameterAst;
                        var param = exportMM.ResolveParameter(paramAst.ParameterName);

                        if (param == null)
                        {
                            i += 1;
                            continue;
                        }

                        if (string.Equals(param.Name, "function", StringComparison.OrdinalIgnoreCase))
                        {
                            // checks for the case of -Function:"verb-nouns"
                            if (paramAst.Argument != null)
                            {
                                exprAst = paramAst.Argument;
                            }
                            // checks for the case of -Function "verb-nouns"
                            else if (i < cmdAst.CommandElements.Count - 1)
                            {
                                i += 1;
                                exprAst = cmdAst.CommandElements[i] as ExpressionAst;
                            }
                        }
                        // some other parameter. we just checks whether the one after this is positional
                        else if (i < cmdAst.CommandElements.Count - 1)
                        {
                            // the next element is a parameter like -module so just move to that one
                            if (cmdAst.CommandElements[i + 1] is CommandParameterAst)
                            {
                                i += 1;
                                continue;
                            }

                            // not a switch parameter so the next element is definitely the argument to this parameter
                            if (paramAst.Argument == null && !switchParams.Contains(param))
                            {
                                // skips the next element
                                i += 1;
                            }

                            i += 1;
                            continue;
                        }
                    }
                    else if (ceAst is ExpressionAst)
                    {
                        exprAst = ceAst as ExpressionAst;
                    }

                    if (exprAst != null)
                    {
                        // One string so just add this to the list
                        if (exprAst is StringConstantExpressionAst)
                        {
                            exportedFunctions.Add((exprAst as StringConstantExpressionAst).Value);
                        }
                        // Array of the form "v-n", "v-n1"
                        else if (exprAst is ArrayLiteralAst)
                        {
                            exportedFunctions.UnionWith(Helper.Instance.GetStringsFromArrayLiteral(exprAst as ArrayLiteralAst));
                        }
                        // Array of the form @("v-n", "v-n1")
                        else if (exprAst is ArrayExpressionAst)
                        {
                            ArrayExpressionAst arrExAst = exprAst as ArrayExpressionAst;
                            if (arrExAst.SubExpression != null && arrExAst.SubExpression.Statements != null)
                            {
                                foreach (StatementAst stAst in arrExAst.SubExpression.Statements)
                                {
                                    if (stAst is PipelineAst)
                                    {
                                        PipelineAst pipeAst = stAst as PipelineAst;
                                        if (pipeAst.PipelineElements != null)
                                        {
                                            foreach (CommandBaseAst cmdBaseAst in pipeAst.PipelineElements)
                                            {
                                                if (cmdBaseAst is CommandExpressionAst)
                                                {
                                                    exportedFunctions.UnionWith(Helper.Instance.GetStringsFromArrayLiteral((cmdBaseAst as CommandExpressionAst).Expression as ArrayLiteralAst));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    i += 1;
                }
            }

            return exportedFunctions;
        }

        /// <summary>
        /// Given a filePath. Returns true if it is a powershell help file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool IsHelpFile(string filePath)
        {
            return filePath != null && File.Exists(filePath) && Path.GetFileName(filePath).StartsWith("about_", StringComparison.OrdinalIgnoreCase)
                && Path.GetFileName(filePath).EndsWith(".help.txt", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Given an AST, checks whether dsc resource is class based or not
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public bool IsDscResourceClassBased(ScriptBlockAst ast)
        {
            if (null == ast)
            {
                return false;
            }

            #if !PSV3

            List<string> dscResourceFunctionNames = new List<string>(new string[] { "Test", "Get", "Set" });

            IEnumerable<Ast> dscClasses = ast.FindAll(item =>
                item is TypeDefinitionAst
                && ((item as TypeDefinitionAst).IsClass)
                && (item as TypeDefinitionAst).Attributes.Any(attr => String.Equals("DSCResource", attr.TypeName.FullName, StringComparison.OrdinalIgnoreCase)), true);

            // Found one or more classes marked with DscResource attribute
            // So this might be a DscResource. Further validation will be performed by the individual rules
            if (null != dscClasses && 0 < dscClasses.Count())
            {
                return true;
            }

            #endif

            return false;
        }

        private string NameWithoutScope(string name, string[] scopes)
        {
            if (String.IsNullOrWhiteSpace(name) || scopes == null)
            {
                return name;
            }            

            // checks whether function name starts with scope
            foreach (string scope in scopes)
            {
                // trim the scope part
                if (name.IndexOf(scope, StringComparison.OrdinalIgnoreCase) == 0) 

                {
                    return name.Substring(scope.Length);
                }
            }

            // no scope
            return name;
        }

        /// <summary>
        /// Given a function name, strip the scope of the name
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public string FunctionNameWithoutScope(string functionName)
        {
            return NameWithoutScope(functionName, functionScopes);
        }

        /// <summary>
        /// Given a variable name, strip the scope
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public string VariableNameWithoutScope(VariablePath variablePath)
        {
            if (variablePath == null || variablePath.UserPath == null)
            {
                return null;
            }

            // strip out the drive if there is one
            if (!string.IsNullOrWhiteSpace(variablePath.DriveName)
                // checks that variable starts with drivename:
                && variablePath.UserPath.IndexOf(string.Concat(variablePath.DriveName, ":")) == 0)
            {
                return variablePath.UserPath.Substring(variablePath.DriveName.Length + 1);
            }

            return NameWithoutScope(variablePath.UserPath, variableScopes);
        }

        /// <summary>
        /// Given a commandast, checks whether it uses splatted variable
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public bool HasSplattedVariable(CommandAst cmdAst)
        {
            if (cmdAst == null || cmdAst.CommandElements == null)
            {
                return false;
            }

            return cmdAst.CommandElements.Any(cmdElem => cmdElem is VariableExpressionAst && (cmdElem as VariableExpressionAst).Splatted);
        }

        /// <summary>
        /// Given a commandast, checks whether positional parameters are used or not.
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <param name="moreThanThreePositional">only return true if more than three positional parameters are used</param>
        /// <returns></returns>
        public bool PositionalParameterUsed(CommandAst cmdAst, bool moreThanThreePositional = false)
        {
            if (cmdAst == null || cmdAst.GetCommandName() == null)
            {
                return false;
            }

            CommandInfo commandInfo = GetCommandInfo(GetCmdletNameFromAlias(cmdAst.GetCommandName())) ?? GetCommandInfo(cmdAst.GetCommandName());

            IEnumerable<ParameterMetadata> switchParams = null;

            if (HasSplattedVariable(cmdAst))
            {
                return false;
            }

            if (commandInfo != null && commandInfo.CommandType == System.Management.Automation.CommandTypes.Cmdlet)
            {
                try
                {
                    switchParams = commandInfo.Parameters.Values.Where<ParameterMetadata>(pm => pm.SwitchParameter);
                }
                catch (Exception)
                {
                    switchParams = null;
                }
            }

            int parameters = 0;
            // Because of the way we count, we will also count the cmdlet as an argument so we have to -1
            int arguments = -1;

            foreach (CommandElementAst ceAst in cmdAst.CommandElements)
            {
                    if (ceAst is CommandParameterAst)
                    {
                        // Skip if it's a switch parameter
                        if (switchParams != null &&
                            switchParams.Any(pm => String.Equals(pm.Name, (ceAst as CommandParameterAst).ParameterName, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }


                        parameters += 1;

                        if ((ceAst as CommandParameterAst).Argument != null)
                        {
                            arguments += 1;
                        }

                    }
                    else
                    {
                        arguments += 1;
                    }
                
            }

            // if not the first element in a pipeline, increase the number of arguments by 1
            PipelineAst parent = cmdAst.Parent as PipelineAst;

            if (parent != null && parent.PipelineElements.Count > 1 && parent.PipelineElements[0] != cmdAst)
            {
                arguments += 1;
            }

            // if we are only checking for 3 or more positional parameters, check that arguments < parameters + 3
            if (moreThanThreePositional && (arguments - parameters) < 3)
            {
                return false;
            }

            return arguments > parameters;
        }

        /// <summary>
        /// Given a command's name, checks whether it exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string name, CommandTypes commandType = CommandTypes.Alias | CommandTypes.Cmdlet | CommandTypes.Configuration | CommandTypes.ExternalScript | CommandTypes.Filter | CommandTypes.Function | CommandTypes.Script | CommandTypes.Workflow)
        {
            lock (getCommandLock)
            {
                return this.invokeCommand.GetCommand(name, commandType);
            }
        }

        /// <summary>
        /// Returns the get, set and test targetresource dsc function
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public IEnumerable<Ast> DscResourceFunctions(Ast ast)
        {
            List<string> resourceFunctionNames = new List<string>(new string[] { "Set-TargetResource", "Get-TargetResource", "Test-TargetResource" });
            return ast.FindAll(item => item is FunctionDefinitionAst
                && resourceFunctionNames.Contains((item as FunctionDefinitionAst).Name, StringComparer.OrdinalIgnoreCase), true);
        }

        /// <summary>
        /// Gets all the strings contained in an array literal ast
        /// </summary>
        /// <param name="alAst"></param>
        /// <returns></returns>
        public List<string> GetStringsFromArrayLiteral(ArrayLiteralAst alAst)
        {
            List<string> result = new List<string>();

            if (alAst != null && alAst.Elements != null)
            {
                foreach (ExpressionAst eAst in alAst.Elements)
                {
                    if (eAst is StringConstantExpressionAst)
                    {
                        result.Add((eAst as StringConstantExpressionAst).Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the block should be skipped as it has a name
        /// that matches keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="namedBlockAst"></param>
        /// <returns></returns>
        public bool SkipBlock(string keyword, Ast namedBlockAst)
        {
            if (namedBlockAst == null)
            {
                return false;
            }

            FindClosingParenthesis(keyword);

            List<Tuple<int, int>> listTuples = KeywordBlockDictionary[keyword];

            if (listTuples == null || listTuples.Count == 0)
            {
                return false;
            }

            int index = listTuples.BinarySearch(Tuple.Create(namedBlockAst.Extent.StartOffset, namedBlockAst.Extent.EndOffset), tupleComparer);

            if (index < 0 || index >= Tokens.Length)
            {
                return false;
            }

            Tuple<int, int> braces = listTuples[index];

            if (braces.Item2 == namedBlockAst.Extent.EndOffset)
            {
                return true;
            }

            return false;
        }

        // Obtain script extent for the function - just around the function name
        public IScriptExtent GetScriptExtentForFunctionName(FunctionDefinitionAst functionDefinitionAst)
        {
            if (null == functionDefinitionAst)
            {
                return null;
            }

            // Obtain the index where the function name is in Tokens
            int funcTokenIndex = Tokens.Select((s, index) => new { s, index })
                          .Where(x => x.s.Extent.StartOffset == functionDefinitionAst.Extent.StartOffset)
                          .Select(x => x.index).FirstOrDefault();

            if (funcTokenIndex > 0 && funcTokenIndex < Helper.Instance.Tokens.Count())
            {
                // return the extent of the next token - this is the extent for the function name
                return Tokens[++funcTokenIndex].Extent;
            }

            return null;
        }
        
        private void FindClosingParenthesis(string keyword)
        {
            if (Tokens == null || Tokens.Length == 0)
            {
                return;
            }

            // Only do this one time per script. The keywordblockdictionary is cleared everytime we run a new script
            if (KeywordBlockDictionary.ContainsKey(keyword))
            {
                return;
            }

            KeywordBlockDictionary[keyword] = new List<Tuple<int, int>>();

            int[] tokenIndices = Tokens
                .Select((token, index) =>
                    String.Equals(token.Text, keyword, StringComparison.OrdinalIgnoreCase) && (token.TokenFlags == TokenFlags.Keyword || token.TokenFlags == TokenFlags.CommandName)
                    ? index : -1)
                .Where(index => index != -1).ToArray();

            foreach (int tokenIndex in tokenIndices)
            {
                int openCurly = -1;

                for (int i = tokenIndex; i < Tokens.Length; i += 1)
                {
                    if (Tokens[i] != null && Tokens[i].Kind == TokenKind.LCurly)
                    {
                        openCurly = i;
                        break;
                    }
                }

                if (openCurly == -1)
                {
                    continue;
                }

                int closeCurly = -1;
                int count = 1;

                for (int i = openCurly + 1; i < Tokens.Length; i += 1)
                {
                    if (Tokens[i] != null)
                    {
                        if (Tokens[i].Kind == TokenKind.LCurly)
                        {
                            count += 1;
                        }
                        else if (Tokens[i].Kind == TokenKind.RCurly)
                        {
                            count -= 1;
                        }
                    }

                    if (count == 0)
                    {
                        closeCurly = i;
                        break;
                    }
                }

                if (closeCurly == -1)
                {
                    continue;
                }

                KeywordBlockDictionary[keyword].Add(Tuple.Create(Tokens[openCurly].Extent.StartOffset,
                    Tokens[closeCurly].Extent.EndOffset));
            }
        }

        /// <summary>
        /// Checks whether the variable VarAst is uninitialized.
        /// </summary>
        /// <param name="varAst"></param>
        /// <param name="ast"></param>
        /// <returns></returns>
        public bool IsUninitialized(VariableExpressionAst varAst, Ast ast)
        {
            if (!VariableAnalysisDictionary.ContainsKey(ast) || VariableAnalysisDictionary[ast] == null)
            {
                return false;
            }

            return VariableAnalysisDictionary[ast].IsUninitialized(varAst);
        }

        /// <summary>
        /// Returns true if varaible is either a global variable or an environment variable
        /// </summary>
        /// <param name="varAst"></param>
        /// <param name="ast"></param>
        /// <returns></returns>
        public bool IsVariableGlobalOrEnvironment(VariableExpressionAst varAst, Ast ast)
        {
            if (!VariableAnalysisDictionary.ContainsKey(ast) || VariableAnalysisDictionary[ast] == null)
            {
                return false;
            }

            return VariableAnalysisDictionary[ast].IsGlobalOrEnvironment(varAst);
        }


        /// <summary>
        /// Checks whether a variable is a global variable.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public bool IsVariableGlobal(VariableExpressionAst varAst)
        {
            //We ignore the use of built-in variable as global variable
            if (varAst.VariablePath.IsGlobal)
            {
                string varName = varAst.VariablePath.UserPath.Remove(varAst.VariablePath.UserPath.IndexOf("global:", StringComparison.OrdinalIgnoreCase), "global:".Length);
                return !SpecialVars.InitializedVariables.Contains(varName, StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }


        /// <summary>
        /// Checks whether all the code path of ast returns.
        /// Runs InitializeVariableAnalysis before calling this method
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public bool AllCodePathReturns(Ast ast)
        {
            if (!VariableAnalysisDictionary.ContainsKey(ast))
            {
                return true;
            }

            var analysis = VariableAnalysisDictionary[ast];
            return analysis.Exit._predecessors.All(block => block._returns || block._unreachable || block._throws);
        }

        /// <summary>
        /// Initialize variable analysis on the script ast
        /// </summary>
        /// <param name="ast"></param>
        public void InitializeVariableAnalysis(Ast ast)
        {
            (new ScriptAnalysis()).AnalyzeScript(ast);
        }

        /// <summary>
        /// Initialize Variable Analysis on Ast ast with variables outside in outerAnalysis
        /// </summary>
        /// <param name="ast"></param>
        internal VariableAnalysis InitializeVariableAnalysisHelper(Ast ast, VariableAnalysis outerAnalysis)
        {
            var VarAnalysis = new VariableAnalysis(new FlowGraph());
            VarAnalysis.AnalyzeImpl(ast, outerAnalysis);
            VariableAnalysisDictionary[ast] = VarAnalysis;
            return VarAnalysis;
        }

        /// <summary>
        /// Get the return type of ret, which is used in function funcAst in scriptAst ast
        /// This function assumes that initialize variable analysis is already run on funcast
        /// It also assumes that the pipeline of ret is not null
        /// </summary>
        /// <param name="funcAst"></param>
        /// <param name="ret"></param>
        /// <param name="classes"></param>
        /// <param name="scriptAst"></param>
        /// <returns></returns>
        
        #if PSV3

        public string GetTypeFromReturnStatementAst(Ast funcAst, ReturnStatementAst ret)

        #else

        public string GetTypeFromReturnStatementAst(Ast funcAst, ReturnStatementAst ret, IEnumerable<TypeDefinitionAst> classes)

        #endif
        {
            if (ret == null || funcAst == null)
            {
                return String.Empty;
            }

            PipelineAst pipe = ret.Pipeline as PipelineAst;

            String result = String.Empty;

            // Handle the case with 1 pipeline element first
            if (pipe != null && pipe.PipelineElements.Count == 1)
            {
                CommandExpressionAst cmAst = pipe.PipelineElements[0] as CommandExpressionAst;
                if (cmAst != null)
                {
                    if (cmAst.Expression.StaticType != typeof(object))
                    {
                        result = cmAst.Expression.StaticType.FullName;
                    }
                    else
                    {
                        VariableExpressionAst varAst = cmAst.Expression as VariableExpressionAst;

                        if (varAst != null)
                        {
                            result = GetVariableTypeFromAnalysis(varAst, funcAst);
                        }
                        else if (cmAst.Expression is MemberExpressionAst)
                        {
                            #if PSV3

                            result = GetTypeFromMemberExpressionAst(cmAst.Expression as MemberExpressionAst, funcAst);

                            #else

                            result = GetTypeFromMemberExpressionAst(cmAst.Expression as MemberExpressionAst, funcAst, classes);

                            #endif
                        }
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(result) && pipe != null && pipe.PipelineElements.Count > 0)
            {
                result = typeof(object).FullName;
            }

            return result;
        }

        /// <summary>
        /// Returns the type from member expression ast, which is inside scopeAst.
        /// This function assumes that Initialize Variable Analysis is already run on scopeAst.
        /// Classes represent the list of DSC classes in the script.
        /// </summary>
        /// <param name="memberAst"></param>
        /// <param name="scopeAst"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        
        #if PSV3

        public string GetTypeFromMemberExpressionAst(MemberExpressionAst memberAst, Ast scopeAst)

        #else

        public string GetTypeFromMemberExpressionAst(MemberExpressionAst memberAst, Ast scopeAst, IEnumerable<TypeDefinitionAst> classes)

        #endif        
        {
            if (memberAst == null)
            {
                return String.Empty;
            }

            VariableAnalysisDetails details = null;

            #if !PSV3

            TypeDefinitionAst psClass = null;

            #endif

            if (memberAst.Expression is VariableExpressionAst && VariableAnalysisDictionary.ContainsKey(scopeAst))
            {
                VariableAnalysis VarTypeAnalysis = VariableAnalysisDictionary[scopeAst];
                // Get the analysis detail for the variable
                details = VarTypeAnalysis.GetVariableAnalysis(memberAst.Expression as VariableExpressionAst);

                #if !PSV3

                if (details != null && classes != null)
                {
                    // Get the class that corresponds to the name of the type (if possible)
                    psClass = classes.FirstOrDefault(item => String.Equals(item.Name, details.Type.FullName, StringComparison.OrdinalIgnoreCase));
                }

                #endif
            }

            #if PSV3

                return GetTypeFromMemberExpressionAstHelper(memberAst, details);

            #else

                return GetTypeFromMemberExpressionAstHelper(memberAst, psClass, details);

            #endif         
        }

        /// <summary>
        /// Retrieves the type from member expression ast. psClass is the powershell class
        /// that represents the type of the object being invoked on (psClass may be null too).
        /// </summary>
        /// <param name="memberAst"></param>
        /// <param name="psClass"></param>
        /// <param name="analysisDetails"></param>
        /// <returns></returns>
        
        #if PSV3
        
        internal string GetTypeFromMemberExpressionAstHelper(MemberExpressionAst memberAst, VariableAnalysisDetails analysisDetails)

        #else

        internal string GetTypeFromMemberExpressionAstHelper(MemberExpressionAst memberAst, TypeDefinitionAst psClass, VariableAnalysisDetails analysisDetails)

        #endif
        {
            //Try to get the type without using psClass first
            Type result = AssignmentTarget.GetTypeFromMemberExpressionAst(memberAst);

            #if !PSV3

            //If we can't get the type, then it may be that the type of the object being invoked on is a powershell class
            if (result == null && psClass != null && analysisDetails != null)
            {
                result = AssignmentTarget.GetTypeFromMemberExpressionAst(memberAst, analysisDetails, psClass);
            }

            #endif

            if (result != null)
            {
                return result.FullName;
            }

            return String.Empty;
        }

        /// <summary>
        /// Get the type of varAst
        /// </summary>
        /// <param name="varAst"></param>
        /// <param name="ast"></param>
        /// <returns></returns>
        public Type GetTypeFromAnalysis(VariableExpressionAst varAst, Ast ast)
        {
            try
            {
                if (VariableAnalysisDictionary.ContainsKey(ast))
                {
                    VariableAnalysis VarTypeAnalysis = VariableAnalysisDictionary[ast];
                    VariableAnalysisDetails details = VarTypeAnalysis.GetVariableAnalysis(varAst);
                    return details.Type;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get type of variable from the variable analysis
        /// </summary>
        /// <param name="varAst"></param>
        /// <param name="ast"></param>
        public string GetVariableTypeFromAnalysis(VariableExpressionAst varAst, Ast ast)
        {
            Type result = GetTypeFromAnalysis(varAst, ast);
            if (result != null)
            {
                return result.FullName;
            }

            return String.Empty;
        }

        /// <summary>
        /// Checks whether the cmdlet parameter is a PS default variable
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public bool HasSpecialVars(string varName)
        {
            if (SpecialVars.InitializedVariables.Contains(varName, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a dictionary of rule suppression from the ast.
        /// Key of the dictionary is rule name.
        /// Value is a list of tuple of integers that represents the interval to apply the rule
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public Dictionary<string, List<RuleSuppression>> GetRuleSuppression(Ast ast)
        {
            List<RuleSuppression> ruleSuppressionList = new List<RuleSuppression>();
            Dictionary<string, List<RuleSuppression>> results = new Dictionary<string, List<RuleSuppression>>(StringComparer.OrdinalIgnoreCase);

            if (ast == null)
            {
                return results;
            }

            ScriptBlockAst sbAst = ast as ScriptBlockAst;

            // Get rule suppression from the ast itself if it is scriptblockast
            if (sbAst != null && sbAst.ParamBlock != null && sbAst.ParamBlock.Attributes != null)
            {
                ruleSuppressionList.AddRange(RuleSuppression.GetSuppressions(sbAst.ParamBlock.Attributes, sbAst.Extent.StartOffset, sbAst.Extent.EndOffset, sbAst));
            }

            // Get rule suppression from functions
            IEnumerable<FunctionDefinitionAst> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>();

            foreach (var funcAst in funcAsts)
            {
                ruleSuppressionList.AddRange(GetSuppressionsFunction(funcAst));
            }

            #if !PSV3

            // Get rule suppression from classes
            IEnumerable<TypeDefinitionAst> typeAsts = ast.FindAll(item => item is TypeDefinitionAst, true).Cast<TypeDefinitionAst>();

            foreach (var typeAst in typeAsts)
            {
                ruleSuppressionList.AddRange(GetSuppressionsClass(typeAst));
            }

            #endif

            ruleSuppressionList.Sort((item, item2) => item.StartOffset.CompareTo(item2.StartOffset));

            foreach (RuleSuppression ruleSuppression in ruleSuppressionList)
            {
                if (!results.ContainsKey(ruleSuppression.RuleName))
                {
                    List<RuleSuppression> ruleSuppressions = new List<RuleSuppression>();
                    results.Add(ruleSuppression.RuleName, ruleSuppressions);
                }

                results[ruleSuppression.RuleName].Add(ruleSuppression);
            }

            return results;
        }

        /// <summary>
        /// Returns a list of rule suppressions from the function
        /// </summary>
        /// <param name="funcAst"></param>
        /// <returns></returns>
        internal List<RuleSuppression> GetSuppressionsFunction(FunctionDefinitionAst funcAst)
        {
            List<RuleSuppression> result = new List<RuleSuppression>();

            if (funcAst != null && funcAst.Body != null
                && funcAst.Body.ParamBlock != null && funcAst.Body.ParamBlock.Attributes != null)
            {
                result.AddRange(RuleSuppression.GetSuppressions(funcAst.Body.ParamBlock.Attributes, funcAst.Extent.StartOffset, funcAst.Extent.EndOffset, funcAst));
            }

            return result;
        }

        /// <summary>
        /// Returns a list of rule suppression from the class
        /// </summary>
        /// <param name="typeAst"></param>
        /// <returns></returns>
        internal List<RuleSuppression> GetSuppressionsClass(TypeDefinitionAst typeAst)
        {
            List<RuleSuppression> result = new List<RuleSuppression>();

            if (typeAst != null && typeAst.Attributes != null && typeAst.Attributes.Count != 0)
            {
                result.AddRange(RuleSuppression.GetSuppressions(typeAst.Attributes, typeAst.Extent.StartOffset, typeAst.Extent.EndOffset, typeAst));
            }

            if (typeAst.Members == null)
            {
                return result;            
            }            

            foreach (var member in typeAst.Members)
            {
                #if PSv3

                FunctionDefinitionAst funcMemb = member as FunctionDefinitionAst;

                #else

                FunctionMemberAst funcMemb = member as FunctionMemberAst;

                #endif

                if (funcMemb == null)
                {
                    continue;
                }

                result.AddRange(RuleSuppression.GetSuppressions(funcMemb.Attributes, funcMemb.Extent.StartOffset, funcMemb.Extent.EndOffset, funcMemb));
            }

            return result;
        }

        /// <summary>
        /// Suppress the rules from the diagnostic records list.
        /// Returns a list of suppressed records as well as the ones that are not suppressed
        /// </summary>
        /// <param name="ruleSuppressions"></param>
        /// <param name="diagnostics"></param>
        public Tuple<List<SuppressedRecord>, List<DiagnosticRecord>> SuppressRule(string ruleName, Dictionary<string, List<RuleSuppression>> ruleSuppressionsDict, List<DiagnosticRecord> diagnostics)
        {
            List<SuppressedRecord> suppressedRecords = new List<SuppressedRecord>();
            List<DiagnosticRecord> unSuppressedRecords = new List<DiagnosticRecord>();
            Tuple<List<SuppressedRecord>, List<DiagnosticRecord>> result = Tuple.Create(suppressedRecords, unSuppressedRecords);

            if (diagnostics == null || diagnostics.Count == 0)
            {
                return result;
            }

            if (ruleSuppressionsDict == null || !ruleSuppressionsDict.ContainsKey(ruleName)
                || ruleSuppressionsDict[ruleName].Count == 0)
            {
                unSuppressedRecords.AddRange(diagnostics);
                return result;
            }

            List<RuleSuppression> ruleSuppressions = ruleSuppressionsDict[ruleName];

            int recordIndex = 0;
            int startRecord = 0;
            bool[] suppressed = new bool[diagnostics.Count];

            foreach (RuleSuppression ruleSuppression in ruleSuppressions)
            {
                int suppressionCount = 0;
                while (startRecord < diagnostics.Count && diagnostics[startRecord].Extent.StartOffset < ruleSuppression.StartOffset)
                {
                    startRecord += 1;
                }

                // at this point, start offset of startRecord is greater or equals to rulesuppression.startoffset
                recordIndex = startRecord;

                while (recordIndex < diagnostics.Count)
                {
                    DiagnosticRecord record = diagnostics[recordIndex];

                    if (record.Extent.EndOffset > ruleSuppression.EndOffset)
                    {
                        break;
                    }

                    // we suppress if there is no suppression id or if there is suppression id and it matches
                    if (string.IsNullOrWhiteSpace(ruleSuppression.RuleSuppressionID)
                        || (!String.IsNullOrWhiteSpace(record.RuleSuppressionID) &&
                            string.Equals(ruleSuppression.RuleSuppressionID, record.RuleSuppressionID, StringComparison.OrdinalIgnoreCase)))
                    {
                        suppressed[recordIndex] = true;
                        suppressedRecords.Add(new SuppressedRecord(record, ruleSuppression));
                        suppressionCount += 1;
                    }

                    recordIndex += 1;
                }

                // If we cannot found any error but the rulesuppression has a rulesuppressionid then it must be used wrongly
                if (!String.IsNullOrWhiteSpace(ruleSuppression.RuleSuppressionID) && suppressionCount == 0)
                {
                    // checks whether are given a string or a file path
                    if (String.IsNullOrWhiteSpace(diagnostics.First().Extent.File))
                    {
                        ruleSuppression.Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormatScriptDefinition, ruleSuppression.StartAttributeLine,
                                String.Format(Strings.RuleSuppressionIDError, ruleSuppression.RuleSuppressionID));
                    }
                    else
                    {
                        ruleSuppression.Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormat, ruleSuppression.StartAttributeLine,
                                System.IO.Path.GetFileName(diagnostics.First().Extent.File), String.Format(Strings.RuleSuppressionIDError, ruleSuppression.RuleSuppressionID));
                    }

                    this.outputWriter.WriteError(new ErrorRecord(new ArgumentException(ruleSuppression.Error), ruleSuppression.Error, ErrorCategory.InvalidArgument, ruleSuppression));
                }
            }

            for (int i = 0; i < suppressed.Length; i += 1)
            {
                if (!suppressed[i])
                {
                    unSuppressedRecords.Add(diagnostics[i]);
                }
            }

            return result;
        }

        public static string[] ProcessCustomRulePaths(string[] rulePaths, SessionState sessionState, bool recurse = false)
        {
            //if directory is given, list all the psd1 files
            List<string> outPaths = new List<string>();
            if (rulePaths == null)
            {
                return null;
            }

            Collection<PathInfo> pathInfo = new Collection<PathInfo>();
            foreach (string rulePath in rulePaths)
            {
                Collection<PathInfo> pathInfosForRulePath = sessionState.Path.GetResolvedPSPathFromPSPath(rulePath);
                if (null != pathInfosForRulePath)
                {
                    foreach (PathInfo pathInfoForRulePath in pathInfosForRulePath)
                    {
                        pathInfo.Add(pathInfoForRulePath);
                    }
                }
            }

            foreach (PathInfo pinfo in pathInfo)
            {
                string path = pinfo.Path;
                if (Directory.Exists(path))
                {
                    path = path.TrimEnd('\\');
                    if (recurse)
                    {
                        outPaths.AddRange(Directory.GetDirectories(pinfo.Path, "*", SearchOption.AllDirectories));
                    }
                }
                outPaths.Add(path);
            }
            
            return outPaths.ToArray();
            
        }


        #endregion
    }


    internal class TupleComparer : IComparer<Tuple<int, int>>
    {
        public int Compare(Tuple<int, int> t1, Tuple<int, int> t2)
        {
            if (t1 == null)
            {
                if (t2 == null)
                {
                    return 0;
                }

                return -1;
            }
            else
            {
                if (t2 == null)
                {
                    return 1;
                }
                else
                {
                    return t1.Item1.CompareTo(t2.Item1);
                }
            }
        }
    }

    /// <summary>
    /// Class used to do variable analysis on the whole script
    /// </summary>
    public class ScriptAnalysis : ICustomAstVisitor
    {
        private VariableAnalysis OuterAnalysis;

        /// <summary>
        /// Analyze the script
        /// </summary>
        /// <param name="ast"></param>
        public void AnalyzeScript(Ast ast)
        {
            if (ast != null)
            {
                ast.Visit(this);
            }
        }

        /// <summary>
        /// Visit Script Block Ast. Sets outeranalysis to the ast before visiting others.
        /// </summary>
        /// <param name="scriptBlockAst"></param>
        /// <returns></returns>
        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            if (scriptBlockAst == null) return null;

            VariableAnalysis previousOuter = OuterAnalysis;

            // We already run variable analysis if the parent is a function so skip these.
            // Otherwise, we have to do variable analysis using the outer scope variables.
            #if PSV3

                if (!(scriptBlockAst.Parent is FunctionDefinitionAst))

            #else

            if (!(scriptBlockAst.Parent is FunctionDefinitionAst) && !(scriptBlockAst.Parent is FunctionMemberAst))

            #endif
            {
                OuterAnalysis = Helper.Instance.InitializeVariableAnalysisHelper(scriptBlockAst, OuterAnalysis);
            }

            if (scriptBlockAst.DynamicParamBlock != null)
            {
                scriptBlockAst.DynamicParamBlock.Visit(this);
            }

            if (scriptBlockAst.BeginBlock != null)
            {
                scriptBlockAst.BeginBlock.Visit(this);
            }

            if (scriptBlockAst.ProcessBlock != null)
            {
                scriptBlockAst.ProcessBlock.Visit(this);
            }

            if (scriptBlockAst.EndBlock != null)
            {
                scriptBlockAst.EndBlock.Visit(this);
            }

            VariableAnalysis innerAnalysis = OuterAnalysis;
            OuterAnalysis = previousOuter;

            #if PSV3

            if (!(scriptBlockAst.Parent is FunctionDefinitionAst))

            #else

            if (!(scriptBlockAst.Parent is FunctionDefinitionAst) && !(scriptBlockAst.Parent is FunctionMemberAst))

            #endif
            {
                // Update the variable analysis of the outer script block
                VariableAnalysis.UpdateOuterAnalysis(OuterAnalysis, innerAnalysis);
            }

            return null;
        }

        /// <summary>
        /// perform special visiting action if statement is a typedefinitionast
        /// </summary>
        /// <param name="statementAst"></param>
        /// <returns></returns>
        private object VisitStatementHelper(StatementAst statementAst)
        {
            if (statementAst == null)
            {
                return null;
            }

            #if PSV3

            statementAst.Visit(this);
            
            #else

            TypeDefinitionAst typeAst = statementAst as TypeDefinitionAst;

            if (typeAst == null)
            {
                statementAst.Visit(this);
                return null;
            }

            foreach (var member in typeAst.Members)
            {
                FunctionMemberAst functionMemberAst = member as FunctionMemberAst;

                if (functionMemberAst != null)
                {
                    var previousOuter = OuterAnalysis;
                    OuterAnalysis = Helper.Instance.InitializeVariableAnalysisHelper(functionMemberAst, OuterAnalysis);

                    if (functionMemberAst != null)
                    {
                        functionMemberAst.Body.Visit(this);
                    }

                    OuterAnalysis = previousOuter;
                }
            }

            #endif

            return null;
        }

        #if !PSV3

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="usingStatement"></param>
        /// <returns></returns>
        public object VisitUsingStatement(UsingStatementAst usingStatement)
        {
            return null;
        }

        #endif

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="arrayExpressionAst"></param>
        /// <returns></returns>
        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="arrayLiteralAst"></param>
        /// <returns></returns>
        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="assignmentStatementAst"></param>
        /// <returns></returns>
        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="attributeAst"></param>
        /// <returns></returns>
        public object VisitAttribute(AttributeAst attributeAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="attributedExpressionAst"></param>
        /// <returns></returns>
        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="binaryExpressionAst"></param>
        /// <returns></returns>
        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body of block statement
        /// </summary>
        /// <param name="blockStatementAst"></param>
        /// <returns></returns>
        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            if (blockStatementAst != null)
            {
                blockStatementAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="breakStatementAst"></param>
        /// <returns></returns>
        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Visits body
        /// </summary>
        /// <param name="catchClauseAst"></param>
        /// <returns></returns>
        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            if (catchClauseAst != null)
            {
                catchClauseAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="commandAst"></param>
        /// <returns></returns>
        public object VisitCommand(CommandAst commandAst)
        {
            if (commandAst == null) return null;

            foreach (CommandElementAst ceAst in commandAst.CommandElements)
            {
                ceAst.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="commandExpressionAst"></param>
        /// <returns></returns>
        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="commandParameterAst"></param>
        /// <returns></returns>
        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="constantExpressionAst"></param>
        /// <returns></returns>
        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="continueStatementAst"></param>
        /// <returns></returns>
        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="convertExpressionAst"></param>
        /// <returns></returns>
        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="dataStatementAst"></param>
        /// <returns></returns>
        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body
        /// </summary>
        /// <param name="doUntilStatementAst"></param>
        /// <returns></returns>
        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            if (doUntilStatementAst != null)
            {
                doUntilStatementAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit body
        /// </summary>
        /// <param name="doWhileStatementAst"></param>
        /// <returns></returns>
        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            if (doWhileStatementAst != null)
            {
                doWhileStatementAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="errorExpressionAst"></param>
        /// <returns></returns>
        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="errorStatementAst"></param>
        /// <returns></returns>
        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="exitStatementAst"></param>
        /// <returns></returns>
        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="expandableStringExpressionAst"></param>
        /// <returns></returns>
        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="fileRedirectionAst"></param>
        /// <returns></returns>
        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body
        /// </summary>
        /// <param name="forEachStatementAst"></param>
        /// <returns></returns>
        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            if (forEachStatementAst != null)
            {
                forEachStatementAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit body
        /// </summary>
        /// <param name="forStatementAst"></param>
        /// <returns></returns>
        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            if (forStatementAst != null)
            {
                forStatementAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Set outer analysis before visiting children
        /// </summary>
        /// <param name="functionDefinitionAst"></param>
        /// <returns></returns>
        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            var outer = OuterAnalysis;
            OuterAnalysis = Helper.Instance.InitializeVariableAnalysisHelper(functionDefinitionAst, OuterAnalysis);

            if (functionDefinitionAst != null)
            {
                functionDefinitionAst.Body.Visit(this);
            }

            OuterAnalysis = outer;
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="hashtableAst"></param>
        /// <returns></returns>
        public object VisitHashtable(HashtableAst hashtableAst)
        {
            return null;
        }

        /// <summary>
        /// Visit the body of each clauses
        /// </summary>
        /// <param name="ifStmtAst"></param>
        /// <returns></returns>
        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            if (ifStmtAst != null)
            {
                if (ifStmtAst.Clauses != null)
                {
                    foreach (var clause in ifStmtAst.Clauses)
                    {
                        if (clause.Item2 != null)
                        {
                            clause.Item2.Visit(this);
                        }
                    }
                }

                if (ifStmtAst.ElseClause != null)
                {
                    ifStmtAst.ElseClause.Visit(this);
                }
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="indexExpressionAst"></param>
        /// <returns></returns>
        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="invokeMemberExpressionAst"></param>
        /// <returns></returns>
        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="memberExpressionAst"></param>
        /// <returns></returns>
        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="mergingRedirectionAst"></param>
        /// <returns></returns>
        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="namedAttributeArgumentAst"></param>
        /// <returns></returns>
        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return null;
        }

        /// <summary>
        /// Visit each statement
        /// </summary>
        /// <param name="namedBlockAst"></param>
        /// <returns></returns>
        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            if (namedBlockAst != null)
            {
                foreach (var statement in namedBlockAst.Statements)
                {
                    VisitStatementHelper(statement);
                }
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="paramBlockAst"></param>
        /// <returns></returns>
        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="parameterAst"></param>
        /// <returns></returns>
        public object VisitParameter(ParameterAst parameterAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="parenExpressionAst"></param>
        /// <returns></returns>
        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit pipeline
        /// </summary>
        /// <param name="pipelineAst"></param>
        /// <returns></returns>
        public object VisitPipeline(PipelineAst pipelineAst)
        {
            if (pipelineAst == null) return null;

            foreach (var command in pipelineAst.PipelineElements)
            {
                command.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="returnStatementAst"></param>
        /// <returns></returns>
        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Visit the scriptblock
        /// </summary>
        /// <param name="scriptBlockExpressionAst"></param>
        /// <returns></returns>
        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            if (scriptBlockExpressionAst != null)
            {
                scriptBlockExpressionAst.ScriptBlock.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit each statement
        /// </summary>
        /// <param name="statementBlockAst"></param>
        /// <returns></returns>
        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            if (statementBlockAst != null)
            {
                foreach (var statement in statementBlockAst.Statements)
                {
                    VisitStatementHelper(statement);
                }
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="stringConstantExpressionAst"></param>
        /// <returns></returns>
        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="subExpressionAst"></param>
        /// <returns></returns>
        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit the body of each clause
        /// </summary>
        /// <param name="switchStatementAst"></param>
        /// <returns></returns>
        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            if (switchStatementAst != null)
            {
                foreach (var clause in switchStatementAst.Clauses)
                {
                    if (clause.Item2 != null)
                    {
                        clause.Item2.Visit(this);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="throwStatementAst"></param>
        /// <returns></returns>
        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="trapStatementAst"></param>
        /// <returns></returns>
        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body, catch and finally
        /// </summary>
        /// <param name="tryStatementAst"></param>
        /// <returns></returns>
        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            if (tryStatementAst != null)
            {
                tryStatementAst.Body.Visit(this);

                if (tryStatementAst.CatchClauses != null)
                {
                    foreach (var clause in tryStatementAst.CatchClauses)
                    {
                        clause.Visit(this);
                    }
                }

                if (tryStatementAst.Finally != null)
                {
                    tryStatementAst.Finally.Visit(this);
                }
            }

            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="typeConstraintAst"></param>
        /// <returns></returns>
        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="typeExpressionAst"></param>
        /// <returns></returns>
        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="unaryExpressionAst"></param>
        /// <returns></returns>
        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="usingExpressionAst"></param>
        /// <returns></returns>
        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="variableExpressionAst"></param>
        /// <returns></returns>
        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body
        /// </summary>
        /// <param name="whileStatementAst"></param>
        /// <returns></returns>
        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            if (whileStatementAst != null)
            {
                whileStatementAst.Body.Visit(this);
            }

            return null;
        }
    }

    /// <summary>
    /// This class is used to find elements in outputted in pipeline.
    /// </summary>
    public class FindPipelineOutput : ICustomAstVisitor
    {
        List<Tuple<string, StatementAst>> outputTypes;

        #if !PSV3

        IEnumerable<TypeDefinitionAst> classes;

        #endif

        FunctionDefinitionAst myFunction;
        /// <summary>
        /// These binary operators will always return boolean value
        /// </summary>
        static TokenKind[] booleanBinaryOperators;

        /// <summary>
        /// These unary operator will return boolean value
        /// </summary>
        static TokenKind[] booleanUnaryOperators;

        static FindPipelineOutput()
        {
            booleanBinaryOperators = new TokenKind[] {
                TokenKind.Icontains,
                TokenKind.Inotcontains,
                TokenKind.Inotin,
                TokenKind.Iin,
                TokenKind.Is,
                TokenKind.IsNot,
                TokenKind.And,
                TokenKind.Or,
                TokenKind.Xor
            };

            booleanUnaryOperators = new TokenKind[] {
                TokenKind.Not,
                TokenKind.Exclaim
            };
        }

        /// <summary>
        /// Find the pipeline output
        /// </summary>
        /// <param name="ast"></param>
        
        #if PSV3

        public FindPipelineOutput(FunctionDefinitionAst ast)

        #else

        public FindPipelineOutput(FunctionDefinitionAst ast, IEnumerable<TypeDefinitionAst> classes)

        #endif
        {
            outputTypes = new List<Tuple<string, StatementAst>>();

            #if !PSV3

            this.classes = classes;

            #endif

            myFunction = ast;

            if (myFunction != null)
            {
                myFunction.Body.Visit(this);
            }
        }

        /// <summary>
        /// Get list of outputTypes from functiondefinitionast funcast
        /// </summary>
        /// <returns></returns>
        
        #if PSV3

        public static List<Tuple<string, StatementAst>> OutputTypes(FunctionDefinitionAst funcAst)
        {
            return (new FindPipelineOutput(funcAst)).outputTypes;
        }

        #else
        public static List<Tuple<string, StatementAst>> OutputTypes(FunctionDefinitionAst funcAst, IEnumerable<TypeDefinitionAst> classes)
        {
            return (new FindPipelineOutput(funcAst, classes)).outputTypes;
        }

        #endif

        /// <summary>
        /// Ignore assignment statement
        /// </summary>
        /// <param name="assignAst"></param>
        /// <returns></returns>
        public object VisitAssignmentStatement(AssignmentStatementAst assignAst)
        {
            return null;
        }

        /// <summary>
        /// Skip NamedAttributeArgumentAst
        /// </summary>
        /// <param name="namedAAAst"></param>
        /// <returns></returns>
        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAAAst)
        {
            return null;
        }

        /// <summary>
        /// Skip Error Expression Ast
        /// </summary>
        /// <param name="errorAst"></param>
        /// <returns></returns>
        public object VisitErrorExpression(ErrorExpressionAst errorAst)
        {
            return null;
        }

        /// <summary>
        /// Skip error statement ast
        /// </summary>
        /// <param name="errorStatementAst"></param>
        /// <returns></returns>
        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Skips function definition ast
        /// </summary>
        /// <param name="functionDefinitionAst"></param>
        /// <returns></returns>
        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return null;
        }

        /// <summary>
        /// Skip ParameterAst
        /// </summary>
        /// <param name="parameterAst"></param>
        /// <returns></returns>
        public object VisitParameter(ParameterAst parameterAst)
        {
            return null;
        }

        /// <summary>
        /// Visit the pipeline of the paren ast
        /// </summary>
        /// <param name="parenAst"></param>
        /// <returns></returns>
        public object VisitParenExpression(ParenExpressionAst parenAst)
        {
            if (parenAst != null)
            {
                return parenAst.Pipeline.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Skips data statement
        /// </summary>
        /// <param name="dataStatementAst"></param>
        /// <returns></returns>
        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            return null;
        }

        /// <summary>
        /// Visit scriptblockast
        /// </summary>
        /// <param name="scriptBlockAst"></param>
        /// <returns></returns>
        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            if (scriptBlockAst != null)
            {
                if (scriptBlockAst.BeginBlock != null)
                {
                    scriptBlockAst.BeginBlock.Visit(this);
                }

                if (scriptBlockAst.ProcessBlock != null)
                {
                    scriptBlockAst.ProcessBlock.Visit(this);
                }

                if (scriptBlockAst.EndBlock != null)
                {
                    scriptBlockAst.EndBlock.Visit(this);
                }
            }

            return null;
        }

        /// <summary>
        /// Visit named block ast. Returns list of types outputted to the stream
        /// </summary>
        /// <param name="namedBlockAst"></param>
        /// <returns></returns>
        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            if (namedBlockAst != null)
            {
                foreach (StatementAst block in namedBlockAst.Statements)
                {
                    object type = block.Visit(this);
                    if (type != null && type is string && !String.IsNullOrWhiteSpace(type as string))
                    {
                        outputTypes.Add(Tuple.Create(type as string, block));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Visit statement block
        /// </summary>
        /// <param name="statementBlockAst"></param>
        /// <returns></returns>
        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            if (statementBlockAst != null)
            {
                foreach (StatementAst block in statementBlockAst.Statements)
                {
                    object type = block.Visit(this);
                    if (type != null && type is string && !String.IsNullOrWhiteSpace(type as string))
                    {
                        outputTypes.Add(Tuple.Create(type as string, block));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Only considers the case where there is one pipeline and it is command expression
        /// </summary>
        /// <param name="pipelineAst"></param>
        /// <returns></returns>
        public object VisitPipeline(PipelineAst pipelineAst)
        {
            // Handle the case with 1 pipeline element
            if (pipelineAst != null && pipelineAst.PipelineElements.Count == 1)
            {
                CommandExpressionAst cmAst = pipelineAst.PipelineElements[0] as CommandExpressionAst;

                if (cmAst != null)
                {
                    return cmAst.Visit(this);
                }
            }

            return null;
        }

        /// <summary>
        /// Visit body of trap
        /// </summary>
        /// <param name="trapAst"></param>
        /// <returns></returns>
        public object VisitTrap(TrapStatementAst trapAst)
        {
            if (trapAst != null)
            {
                return trapAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// In all the clauses, we skip the first item
        /// </summary>
        /// <param name="ifStatementAst"></param>
        /// <returns></returns>
        public object VisitIfStatement(IfStatementAst ifStatementAst)
        {
            if (ifStatementAst == null || ifStatementAst.Clauses == null || ifStatementAst.Clauses.Count == 0)
            {
                return null;
            }

            foreach (var clause in ifStatementAst.Clauses)
            {
                clause.Item2.Visit(this);
            }

            if (ifStatementAst.ElseClause != null)
            {
                ifStatementAst.ElseClause.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit SwitchStatement. Skip the condition
        /// </summary>
        /// <param name="switchStatementAst"></param>
        /// <returns></returns>
        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            if (switchStatementAst == null || switchStatementAst.Clauses == null || switchStatementAst.Clauses.Count == 0)
            {
                return null;
            }

            foreach (var clause in switchStatementAst.Clauses)
            {
                // Skip item 1
                clause.Item2.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit foreach statement. Skip condition
        /// </summary>
        /// <param name="loopStatementAst"></param>
        /// <returns></returns>
        public object VisitForEachStatement(ForEachStatementAst foreachAst)
        {
            if (foreachAst != null)
            {
                foreachAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit Do While Statement. Skip Condition
        /// </summary>
        /// <param name="doWhileAst"></param>
        /// <returns></returns>
        public object VisitDoWhileStatement(DoWhileStatementAst doWhileAst)
        {
            if (doWhileAst != null)
            {
                doWhileAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit Do Until Statement. Skip Condition
        /// </summary>
        /// <param name="doWhileAst"></param>
        /// <returns></returns>
        public object VisitDoUntilStatement(DoUntilStatementAst doUntilAst)
        {
            if (doUntilAst != null)
            {
                doUntilAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit While Statement. Skip Condition
        /// </summary>
        /// <param name="doWhileAst"></param>
        /// <returns></returns>
        public object VisitWhileStatement(WhileStatementAst whileAst)
        {
            if (whileAst != null)
            {
                whileAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit forstatement. Skip Condition, Initializer and Iterator
        /// </summary>
        /// <param name="forAst"></param>
        /// <returns></returns>
        public object VisitForStatement(ForStatementAst forAst)
        {
            if (forAst != null)
            {
                forAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Skip command ast
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public object VisitCommand(CommandAst cmdAst)
        {
            return null;
        }

        /// <summary>
        /// Skip if type of convert is void
        /// </summary>
        /// <param name="convAst"></param>
        /// <returns></returns>
        public object VisitConvertExpression(ConvertExpressionAst convAst)
        {
            if (convAst != null)
            {
                if (convAst.Type.TypeName.GetReflectionType() != null)
                {
                    return convAst.Type.TypeName.GetReflectionType().FullName;
                }

                return convAst.Type.TypeName.FullName;
            }

            return null;
        }

        /// <summary>
        /// Skip fileRedirectionAst
        /// </summary>
        /// <param name="fileRedirectionAst"></param>
        /// <returns></returns>
        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            return null;
        }

        /// <summary>
        /// Visit script block expression
        /// </summary>
        /// <param name="scriptBlockAst"></param>
        /// <returns></returns>
        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockAst)
        {
            if (scriptBlockAst != null)
            {
                return scriptBlockAst.ScriptBlock.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Skip fileRedirectionAst
        /// </summary>
        /// <param name="fileRedirectionAst"></param>
        /// <returns></returns>
        public object VisitMergingRedirection(MergingRedirectionAst mergingAst)
        {
            return null;
        }

        /// <summary>
        /// Returns type of type constraint ast
        /// </summary>
        /// <param name="typeAst"></param>
        /// <returns></returns>
        public object VisitTypeConstraint(TypeConstraintAst typeAst)
        {
            if (typeAst != null)
            {
                if (typeAst.TypeName.GetReflectionType() != null)
                {
                    return typeAst.TypeName.GetReflectionType().FullName;
                }

                return typeAst.TypeName.FullName;
            }

            return null;
        }

        /// <summary>
        /// Skip throw statement.
        /// </summary>
        /// <param name="throwAst"></param>
        /// <returns></returns>
        public object VisitThrowStatement(ThrowStatementAst throwAst)
        {
            return null;
        }

        /// <summary>
        /// Returns type of typeExpressionAst
        /// </summary>
        /// <param name="typeExpressionAst"></param>
        /// <returns></returns>
        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            if (typeExpressionAst != null)
            {
                if (typeExpressionAst.TypeName.GetReflectionType() != null)
                {
                    return typeExpressionAst.TypeName.GetReflectionType().FullName;
                }

                return typeExpressionAst.TypeName.FullName;
            }

            return null;
        }

        /// <summary>
        /// This is where we can get the type
        /// </summary>
        /// <param name="commandAst"></param>
        /// <returns></returns>
        public object VisitCommandExpression(CommandExpressionAst commandAst)
        {
            if (commandAst != null)
            {
                return commandAst.Expression.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Return the type of return statement
        /// </summary>
        /// <param name="returnStatementAst"></param>
        /// <returns></returns>
        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            #if PSV3

            return Helper.Instance.GetTypeFromReturnStatementAst(myFunction, returnStatementAst);

            #else

            return Helper.Instance.GetTypeFromReturnStatementAst(myFunction, returnStatementAst, classes);

            #endif            
        }

        /// <summary>
        /// Returns the type of memberexpressionast
        /// </summary>
        /// <param name="memAst"></param>
        /// <returns></returns>
        public object VisitMemberExpression(MemberExpressionAst memAst)
        {
            #if PSV3

            return Helper.Instance.GetTypeFromMemberExpressionAst(memAst, myFunction);

            #else

            return Helper.Instance.GetTypeFromMemberExpressionAst(memAst, myFunction, classes);

            #endif
        }

        /// <summary>
        /// Returns the type of invoke member expression ast
        /// </summary>
        /// <param name="invokeAst"></param>
        /// <returns></returns>
        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeAst)
        {
            #if PSV3

            return Helper.Instance.GetTypeFromMemberExpressionAst(invokeAst, myFunction);

            #else

            return Helper.Instance.GetTypeFromMemberExpressionAst(invokeAst, myFunction, classes);

            #endif
        }

        /// <summary>
        /// Visit a string constantexpressionast
        /// </summary>
        /// <param name="strAst"></param>
        /// <returns></returns>
        public object VisitStringConstantExpression(StringConstantExpressionAst strAst)
        {
            if (strAst != null)
            {
                return strAst.StaticType.FullName;
            }

            return null;
        }

        /// <summary>
        /// Skip command parameter
        /// </summary>
        /// <param name="cmdParamAst"></param>
        /// <returns></returns>
        public object VisitCommandParameter(CommandParameterAst cmdParamAst)
        {
            return null;
        }

        /// <summary>
        /// Visit a constantexpressionast
        /// </summary>
        /// <param name="constantExpressionAst"></param>
        /// <returns></returns>
        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            if (constantExpressionAst != null)
            {
                return constantExpressionAst.StaticType.FullName;
            }

            return null;
        }

        /// <summary>
        /// Skip break statement ast
        /// </summary>
        /// <param name="breakAst"></param>
        /// <returns></returns>
        public object VisitBreakStatement(BreakStatementAst breakAst)
        {
            return null;
        }

        /// <summary>
        /// Visit body, catch and finally clause
        /// </summary>
        /// <param name="tryAst"></param>
        /// <returns></returns>
        public object VisitTryStatement(TryStatementAst tryAst)
        {
            if (tryAst != null)
            {
                if (tryAst.Body != null)
                {
                    tryAst.Body.Visit(this);
                }

                if (tryAst.CatchClauses != null)
                {
                    foreach (var catchClause in tryAst.CatchClauses)
                    {
                        catchClause.Visit(this);
                    }
                }

                if (tryAst.Finally != null)
                {
                    tryAst.Finally.Visit(this);
                }
            }

            return null;
        }

        /// <summary>
        /// Visit body of catch clause
        /// </summary>
        /// <param name="catchAst"></param>
        /// <returns></returns>
        public object VisitCatchClause(CatchClauseAst catchAst)
        {
            if (catchAst != null)
            {
                return catchAst.Body.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Skip continue statement;
        /// </summary>
        /// <param name="contAst"></param>
        /// <returns></returns>
        public object VisitContinueStatement(ContinueStatementAst contAst)
        {
            return null;
        }

        public object VisitSubExpression(SubExpressionAst subExprAst)
        {
            if (subExprAst != null)
            {
                return subExprAst.SubExpression.Visit(this);
            }

            return null;
        }

        /// <summary>
        /// Visit the body of blockstatementast
        /// </summary>
        /// <param name="blockAst"></param>
        /// <returns></returns>
        public object VisitBlockStatement(BlockStatementAst blockAst)
        {
            return blockAst.Body.Visit(this);
        }

        /// <summary>
        /// Returns type of array
        /// </summary>
        /// <param name="arrayExprAst"></param>
        /// <returns></returns>
        public object VisitArrayExpression(ArrayExpressionAst arrayExprAst)
        {
            return typeof(System.Array).FullName;
        }

        /// <summary>
        /// Returns type of array
        /// </summary>
        /// <param name="arrayLiteral"></param>
        /// <returns></returns>
        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteral)
        {
            return typeof(System.Array).FullName;
        }

        /// <summary>
        /// Returns type of hashtable
        /// </summary>
        /// <param name="hashtableAst"></param>
        /// <returns></returns>
        public object VisitHashtable(HashtableAst hashtableAst)
        {
            return typeof(System.Collections.Hashtable).FullName;
        }

        /// <summary>
        /// Returns type of variable
        /// </summary>
        /// <param name="varExpressionAst"></param>
        /// <returns></returns>
        public object VisitVariableExpression(VariableExpressionAst varExpressionAst)
        {
            return Helper.Instance.GetVariableTypeFromAnalysis(varExpressionAst, myFunction);
        }

        /// <summary>
        /// Return string type
        /// </summary>
        /// <param name="expandableStringAst"></param>
        /// <returns></returns>
        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringAst)
        {
            return typeof(string).FullName;
        }

        /// <summary>
        /// Skip exit statement ast
        /// </summary>
        /// <param name="exitAst"></param>
        /// <returns></returns>
        public object VisitExitStatement(ExitStatementAst exitAst)
        {
            return null;
        }

        /// <summary>
        /// Visit attributedexpression
        /// </summary>
        /// <param name="attrExpr"></param>
        /// <returns></returns>
        public object VisitAttributedExpression(AttributedExpressionAst attrExpr)
        {
            return null;
        }

        /// <summary>
        /// Skip attribute ast
        /// </summary>
        /// <param name="attrAst"></param>
        /// <returns></returns>
        public object VisitAttribute(AttributeAst attrAst)
        {
            return null;
        }

        /// <summary>
        /// Skip param block
        /// </summary>
        /// <param name="paramBlockAst"></param>
        /// <returns></returns>
        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return null;
        }

        /// <summary>
        /// Return type of the index expression
        /// </summary>
        /// <param name="indexAst"></param>
        /// <returns></returns>
        public object VisitIndexExpression(IndexExpressionAst indexAst)
        {
            if (indexAst != null && indexAst.Target is VariableExpressionAst)
            {
                Type type = Helper.Instance.GetTypeFromAnalysis(indexAst.Target as VariableExpressionAst, myFunction);
                if (type != null)
                {
                    Type elemType = type.GetElementType();
                    if (elemType != null)
                    {
                        return elemType.FullName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Only returns boolean type for unary operator that returns boolean
        /// </summary>
        /// <param name="unaryAst"></param>
        /// <returns></returns>
        public object VisitUnaryExpression(UnaryExpressionAst unaryAst)
        {
            if (unaryAst != null && booleanUnaryOperators.Contains(unaryAst.TokenKind))
            {
                return typeof(bool).FullName;
            }

            return null;
        }

        /// <summary>
        /// Only returns boolean type for binary operator that returns boolean
        /// </summary>
        /// <param name="binAst"></param>
        /// <returns></returns>
        public object VisitBinaryExpression(BinaryExpressionAst binAst)
        {
            if (binAst != null && booleanBinaryOperators.Contains(binAst.Operator))
            {
                return typeof(bool).FullName;
            }

            return null;
        }

        /// <summary>
        /// Skips using expression ast
        /// </summary>
        /// <param name="usingExpressionAst"></param>
        /// <returns></returns>
        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return null;
        }
    }
}
