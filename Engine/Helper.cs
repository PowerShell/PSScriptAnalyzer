using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer
{

    /// <summary>
    /// This Helper class contains utility/helper functions for classes in ScriptAnalyzer.
    /// </summary>
    public class Helper
    {
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
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Helper();
                    }
                }

                return instance;
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

        /// <summary>
        /// ScriptAnalyzer Cmdlet, used for getting commandinfos of other commands.
        /// </summary>
        public PSCmdlet MyCmdlet { get; set; }

        private TupleComparer tupleComparer = new TupleComparer();

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

        #endregion

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

            IEnumerable<CommandInfo> aliases = MyCmdlet.InvokeCommand.GetCommands("*", CommandTypes.Alias, true);

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
        /// Given a commandast, checks whether positional parameters are used or not.
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public bool PositionalParameterUsed(CommandAst cmdAst)
        {
            if (cmdAst == null || cmdAst.GetCommandName() == null)
            {
                return false;
            }

            CommandInfo commandInfo = GetCommandInfo(GetCmdletNameFromAlias(cmdAst.GetCommandName())) ?? GetCommandInfo(cmdAst.GetCommandName());

            IEnumerable<ParameterMetadata> switchParams = null;
            IEnumerable<CommandParameterSetInfo> scriptBlocks = null;
            bool hasScriptBlockSet = false;

            if (commandInfo != null && commandInfo.CommandType == System.Management.Automation.CommandTypes.Cmdlet)
            {
                try
                {
                    switchParams = commandInfo.Parameters.Values.Where<ParameterMetadata>(pm => pm.SwitchParameter);
                    scriptBlocks = commandInfo.ParameterSets;
                    foreach (CommandParameterSetInfo cmdParaset in scriptBlocks)
                    {
                        if (String.Equals(cmdParaset.Name, "ScriptBlockSet", StringComparison.OrdinalIgnoreCase))
                        {
                            hasScriptBlockSet = true;
                        }
                    }

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
                if (!hasScriptBlockSet)
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
                        //Skip if splatting "@" is used
                        if (ceAst is VariableExpressionAst)
                        {
                            if ((ceAst as VariableExpressionAst).Splatted)
                            {
                                continue;
                            }
                        }
                        arguments += 1;
                    }
                }
            }

            return arguments > parameters;
        }

        /// <summary>
        /// Given a command's name, checks whether it exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string name)
        {
            return Helper.Instance.MyCmdlet.InvokeCommand.GetCommand(name, CommandTypes.All);
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
        /// Initialize Variable Analysis on Ast ast
        /// </summary>
        /// <param name="ast"></param>
        public void InitializeVariableAnalysis(Ast ast)
        {
            if (VariableAnalysisDictionary.ContainsKey(ast))
            {
                return;
            }

            try
            {
                var VarAnalysis = new VariableAnalysis(new FlowGraph());
                VarAnalysis.AnalyzeImpl(ast);
                VariableAnalysisDictionary[ast] = VarAnalysis;
            }
            catch { }
        }


        /// <summary>
        /// Get type of variable from the variable analysis
        /// </summary>
        /// <param name="varAst"></param>
        /// <param name="ast"></param>
        public string GetVariableTypeFromAnalysis(VariableExpressionAst varAst, Ast ast)
        {
            try
            {
                if (VariableAnalysisDictionary.ContainsKey(ast))
                {
                    VariableAnalysis VarTypeAnalysis = VariableAnalysisDictionary[ast];
                    VariableAnalysisDetails details = VarTypeAnalysis.GetVariableAnalysis(varAst);
                    return details.Type.FullName;
                }
                else
                {
                    return "";
                }
            }
            catch {
                return "";
            }
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
}
