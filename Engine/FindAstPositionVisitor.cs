// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides an efficient way to find the position in the AST corresponding to a given script position.
    /// </summary>
#if !(PSV3 || PSV4)
    internal class FindAstPositionVisitor : AstVisitor2
#else
    internal class FindAstPositionVisitor : AstVisitor
#endif
    {
        private IScriptPosition searchPosition;

        /// <summary>
        /// Contains the position in the AST corresponding to the provided <see cref="IScriptPosition"/> upon completion of the <see cref="Ast.Visit(AstVisitor)"/> method.
        /// </summary>
        public Ast AstPosition { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FindAstPositionVisitor"/> class with the postition to search for.
        /// </summary>
        /// <param name="position">The script position to search for.</param>
        public FindAstPositionVisitor(IScriptPosition position)
        {
            this.searchPosition = position;
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
         {
             return Visit(arrayExpressionAst);
         }

         public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
         {
             return Visit(arrayLiteralAst);
         }

         public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
         {
             return Visit(assignmentStatementAst);
         }

         public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
         {
             return Visit(attributeAst);
         }

         public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
         {
             return Visit(attributedExpressionAst);
         }

         public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
         {
             return Visit(binaryExpressionAst);
         }

         public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
         {
             return Visit(blockStatementAst);
         }

         public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
         {
             return Visit(breakStatementAst);
         }

         public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
         {
             return Visit(catchClauseAst);
         }

         public override AstVisitAction VisitCommand(CommandAst commandAst)
         {
             return Visit(commandAst);
         }

         public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
         {
             return Visit(commandExpressionAst);
         }

         public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
         {
             return Visit(commandParameterAst);
         }

         public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
         {
             return Visit(constantExpressionAst);
         }

         public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
         {
             return Visit(continueStatementAst);
         }

         public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
         {
             return Visit(convertExpressionAst);
         }

         public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
         {
             return Visit(dataStatementAst);
         }

         public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
         {
             return Visit(doUntilStatementAst);
         }

         public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
         {
             return Visit(doWhileStatementAst);
         }

         public override AstVisitAction VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
         {
             return Visit(errorExpressionAst);
         }

         public override AstVisitAction VisitErrorStatement(ErrorStatementAst errorStatementAst)
         {
             return Visit(errorStatementAst);
         }

         public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
         {
             return Visit(exitStatementAst);
         }

         public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
         {
             return Visit(expandableStringExpressionAst);
         }

         public override AstVisitAction VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
         {
             return Visit(fileRedirectionAst);
         }

         public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
         {
             return Visit(forEachStatementAst);
         }

         public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
         {
             return Visit(forStatementAst);
         }

         public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
         {
             return Visit(functionDefinitionAst);
         }

         public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
         {
             return Visit(hashtableAst);
         }

         public override AstVisitAction VisitIfStatement(IfStatementAst ifStatementAst)
         {
             return Visit(ifStatementAst);
         }

         public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
         {
             return Visit(indexExpressionAst);
         }

         public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
         {
             return Visit(invokeMemberExpressionAst);
         }

         public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
         {
             return Visit(memberExpressionAst);
         }

         public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
         {
             return Visit(mergingRedirectionAst);
         }

         public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
         {
             return Visit(namedAttributeArgumentAst);
         }

         public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
         {
             return Visit(namedBlockAst);
         }

         public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
         {
             return Visit(paramBlockAst);
         }

         public override AstVisitAction VisitParameter(ParameterAst parameterAst)
         {
             return Visit(parameterAst);
         }

         public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
         {
             return Visit(parenExpressionAst);
         }

         public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
         {
             return Visit(pipelineAst);
         }

         public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
         {
             return Visit(returnStatementAst);
         }

         public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
         {
             return Visit(scriptBlockAst);
         }

         public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
         {
             return Visit(scriptBlockExpressionAst);
         }

         public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
         {
             return Visit(statementBlockAst);
         }

         public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
         {
             return Visit(stringConstantExpressionAst);
         }

         public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
         {
             return Visit(subExpressionAst);
         }

         public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
         {
             return Visit(switchStatementAst);
         }

         public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
         {
             return Visit(throwStatementAst);
         }

         public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
         {
             return Visit(trapStatementAst);
         }

         public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
         {
             return Visit(tryStatementAst);
         }

         public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
         {
             return Visit(typeConstraintAst);
         }

         public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
         {
             return Visit(typeExpressionAst);
         }

         public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
         {
             return Visit(unaryExpressionAst);
         }

         public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
         {
             return Visit(usingExpressionAst);
         }

         public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
         {
             return Visit(variableExpressionAst);
         }

         public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
         {
             return Visit(whileStatementAst);
         }

#if !(PSV3 || PSV4)
         public override AstVisitAction VisitBaseCtorInvokeMemberExpression(BaseCtorInvokeMemberExpressionAst baseCtorInvokeMemberExpressionAst)
         {
             return Visit(baseCtorInvokeMemberExpressionAst);
         }

         public override AstVisitAction VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
         {
             return Visit(configurationDefinitionAst);
         }

         public override AstVisitAction VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordStatementAst)
         {
             return Visit(dynamicKeywordStatementAst);
         }

         public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
         {
             return Visit(functionMemberAst);
         }

         public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
         {
             return Visit(propertyMemberAst);
         }

         public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
         {
             return Visit(typeDefinitionAst);
         }

         public override AstVisitAction VisitUsingStatement(UsingStatementAst usingStatementAst)
         {
             return AstVisitAction.Continue;
         }
#endif

#if !(NET462 || PSV7) // net462 includes V3,4,5
         public override AstVisitAction VisitPipelineChain(PipelineChainAst pipelineChainAst)
         {
             return Visit(pipelineChainAst);
         }

         public override AstVisitAction VisitTernaryExpression(TernaryExpressionAst ternaryExpressionAst)
         {
             return Visit(ternaryExpressionAst);
         }
#endif

        /// <summary>
        /// Traverses the AST based on offests to find the leaf-most node which contains the provided <see cref="IScriptPosition"/>.
        /// This method implements the entire functionality of this visitor. All <see cref="AstVisitor2"/> methods are overridden to simply invoke this one.
        /// </summary>
        /// <param name="ast">Current AST node to process.</param>
        /// <returns>An <see cref="AstVisitAction"/> indicating whether to visit children of the current node.</returns>
        private AstVisitAction Visit(Ast ast)
        {
            if (ast.Extent.StartOffset > searchPosition.Offset || ast.Extent.EndOffset <= searchPosition.Offset)
            {
                return AstVisitAction.SkipChildren;
            }
            AstPosition = ast;
            return AstVisitAction.Continue;
        }

    }
}