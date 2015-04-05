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
        /// Get the return type of ret, which is used in function funcAst in scriptAst ast
        /// This function assumes that initialize variable analysis is already run on funcast
        /// It also assumes that the pipeline of ret is not null
        /// </summary>
        /// <param name="funcAst"></param>
        /// <param name="ret"></param>
        /// <param name="classes"></param>
        /// <param name="scriptAst"></param>
        /// <returns></returns>
        public string GetTypeFromReturnStatementAst(Ast funcAst, ReturnStatementAst ret, IEnumerable<TypeDefinitionAst> classes)
        {
            if (ret == null || funcAst == null)
            {
                return String.Empty;
            }

            PipelineAst pipe = ret.Pipeline as PipelineAst;

            // Handle the case with 1 pipeline element first
            if (pipe != null && pipe.PipelineElements.Count == 1)
            {
                CommandExpressionAst cmAst = pipe.PipelineElements[0] as CommandExpressionAst;
                if (cmAst != null)
                {
                    if (cmAst.Expression.StaticType != typeof(object))
                    {
                        return cmAst.Expression.StaticType.FullName;
                    }

                    VariableExpressionAst varAst = cmAst.Expression as VariableExpressionAst;

                    if (varAst != null)
                    {
                        return GetVariableTypeFromAnalysis(varAst, funcAst);
                    }

                    if (cmAst.Expression is MemberExpressionAst)
                    {
                        return GetTypeFromMemberExpressionAstHelper(cmAst.Expression as MemberExpressionAst, funcAst, classes);
                    }
                }
            }

            return String.Empty;
        }

        internal string GetTypeFromMemberExpressionAstHelper(MemberExpressionAst memberAst, Ast scopeAst, IEnumerable<TypeDefinitionAst> classes)
        {
            if (memberAst == null)
            {
                return String.Empty;
            }

            VariableAnalysisDetails details = null;
            TypeDefinitionAst psClass = null;

            if (memberAst.Expression is VariableExpressionAst && VariableAnalysisDictionary.ContainsKey(scopeAst))
            {
                VariableAnalysis VarTypeAnalysis = VariableAnalysisDictionary[scopeAst];
                details = VarTypeAnalysis.GetVariableAnalysis(memberAst.Expression as VariableExpressionAst);

                if (details != null && classes != null)
                {
                    psClass = classes.FirstOrDefault(item => String.Equals(item.Name, details.Type.FullName, StringComparison.OrdinalIgnoreCase));
                }
            }

            return GetTypeFromMemberExpressionAst(memberAst, psClass, details);
        }

        /// <summary>
        /// Retrieves the type from member expression ast
        /// </summary>
        /// <param name="memberAst"></param>
        /// <param name="psClass"></param>
        /// <param name="analysisDetails"></param>
        /// <returns></returns>
        public string GetTypeFromMemberExpressionAst(MemberExpressionAst memberAst, TypeDefinitionAst psClass, VariableAnalysisDetails analysisDetails)
        {
            Type result = AssignmentTarget.GetTypeFromMemberExpressionAst(memberAst);

            if (result == null && psClass != null && analysisDetails != null)
            {
                result = AssignmentTarget.GetTypeFromMemberExpressionAst(memberAst, analysisDetails, psClass);
            }

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
        internal Type GetTypeFromAnalysis(VariableExpressionAst varAst, Ast ast)
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
    /// This class is used to find elements in outputted in pipeline.
    /// </summary>
    public class FindPipelineOutput : ICustomAstVisitor
    {
        List<string> outputTypes;

        IEnumerable<TypeDefinitionAst> classes;

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
        public FindPipelineOutput(FunctionDefinitionAst ast, IEnumerable<TypeDefinitionAst> classes)
        {
            outputTypes = new List<string>();
            Helper.Instance.InitializeVariableAnalysis(ast);
            this.classes = classes;
            myFunction = ast;

            if (myFunction != null)
            {
                myFunction.Body.Visit(this);
            }
        }

        /// <summary>
        /// Get list of outputTypes
        /// </summary>
        /// <returns></returns>
        public List<string> OutputTypes
        {
            get
            {
                return outputTypes;
            }
        }

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
                foreach (var block in namedBlockAst.Statements)
                {
                    object type = block.Visit(this);
                    if (type != null && type is string && !String.IsNullOrWhiteSpace(type as string))
                    {
                        outputTypes.Add(type as string);
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
                foreach (var block in statementBlockAst.Statements)
                {
                    object type = block.Visit(this);
                    if (type != null && type is string && !String.IsNullOrWhiteSpace(type as string))
                    {
                        outputTypes.Add(type as string);
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
            return Helper.Instance.GetTypeFromReturnStatementAst(myFunction, returnStatementAst, classes);
        }

        /// <summary>
        /// Returns the type of memberexpressionast
        /// </summary>
        /// <param name="memAst"></param>
        /// <returns></returns>
        public object VisitMemberExpression(MemberExpressionAst memAst)
        {
            return Helper.Instance.GetTypeFromMemberExpressionAstHelper(memAst, myFunction, classes);
        }

        /// <summary>
        /// Returns the type of invoke member expression ast
        /// </summary>
        /// <param name="invokeAst"></param>
        /// <returns></returns>
        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeAst)
        {
            return Helper.Instance.GetTypeFromMemberExpressionAstHelper(invokeAst, myFunction, classes);
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
