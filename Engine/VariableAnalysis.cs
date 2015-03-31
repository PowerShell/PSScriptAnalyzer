/********************************************************************++
Copyright (c) Microsoft Corporation.  All rights reserved.
--********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer
{
    /// <summary>
    /// This class is used to analyze variable based on data flow
    /// </summary>
    public class VariableAnalysis : AnalysisBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="decorated"></param>
        public VariableAnalysis(IFlowGraph decorated) : base(decorated) { }

        private Dictionary<string, VariableAnalysisDetails> _variables;
        private readonly List<LoopGotoTargets> _loopTargets = new List<LoopGotoTargets>();
        private Dictionary<string, VariableAnalysisDetails> VariablesDictionary;

        /// <summary>
        /// Used to analyze scriptbloct, functionmemberast or functiondefinitionast
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public static void Analyze(Ast ast)
        {
            if (ast == null) return;

            (new VariableAnalysis(new FlowGraph())).AnalyzeImpl(ast);
        }

        /// <summary>
        /// Analyze a member function, marking variable references as "dynamic" (so they can be reported as errors)
        /// and also analyze the control flow to make sure every block returns (or throws)
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public static bool AnalyzeMemberFunction(Ast ast)
        {
            VariableAnalysis va = (new VariableAnalysis(new FlowGraph()));
            va.AnalyzeImpl(ast);
            return va.Exit._predecessors.All(b => b._returns || b._throws || b._unreachable);
        }

        /// <summary>
        /// Return parameters of a functionmemberast or functiondefinitionast
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<ParameterAst> FindParameters(Ast ast, Type type)
        {
            IEnumerable<Ast> parameters = ast.FindAll(item => item is ParameterAst, true);

            foreach (ParameterAst parameter in parameters)
            {
                Ast parent = parameter.Parent;
                while (parent != null)
                {
                    if (parent.GetType() == type)
                    {
                        if (parent != ast)
                        {
                            break;
                        }

                        yield return parameter;
                    }

                    parent = parent.Parent;
                }
            }
        }

        private Dictionary<String, VariableTarget> ProcessParameters(IEnumerable<ParameterAst> parameters)
        {
            Dictionary<String, VariableTarget> varTargets = new Dictionary<String, VariableTarget>();

            foreach (var parameter in parameters)
            {
                var variablePath = parameter.Name.VariablePath;
                bool isSwitchOrMandatory = false;
                Type type = null;
                foreach (var paramAst in parameter.Attributes)
                {
                    if (paramAst is TypeConstraintAst)
                    {
                        if (type == null)
                        {
                            type = paramAst.TypeName.GetReflectionType();
                        }

                        if (String.Equals(paramAst.TypeName.FullName, "switch", StringComparison.OrdinalIgnoreCase))
                        {
                            isSwitchOrMandatory = true;
                        }
                    }
                    else if (paramAst is AttributeAst)
                    {
                        var args = (paramAst as AttributeAst).NamedArguments;
                        if (args != null)
                        {
                            foreach (NamedAttributeArgumentAst arg in args)
                            {
                                if (String.Equals(arg.ArgumentName, "mandatory", StringComparison.OrdinalIgnoreCase)
                                    && String.Equals(arg.Argument.Extent.Text, "$true", StringComparison.OrdinalIgnoreCase))
                                {
                                    isSwitchOrMandatory = true;
                                }
                            }
                        }
                    }
                }

                var varName = AssignmentTarget.GetUnaliasedVariableName(variablePath);
                var details = _variables[varName];
                type = type ?? details.Type ?? typeof(object);

                if (parameter.DefaultValue != null)
                {
                    var assignTarget = new AssignmentTarget(varName, type);

                    if (parameter.DefaultValue is ConstantExpressionAst)
                    {
                        assignTarget.Constant = (parameter.DefaultValue as ConstantExpressionAst).Value;
                        assignTarget.Type = assignTarget.Constant == null ? typeof(object) : assignTarget.Constant.GetType();
                    }

                    Entry.AddAst(assignTarget);
                }
                else if (isSwitchOrMandatory)
                {
                    // Consider switch or mandatory parameter as already initialized
                    Entry.AddAst(new AssignmentTarget(varName, type));
                }
                else if (type != typeof(object))
                {
                    VariableTarget varTarget = new VariableTarget(parameter.Name);
                    varTarget.Type = type;
                    if (!varTargets.ContainsKey(varTarget.Name))
                    {
                        varTargets.Add(varTarget.Name, varTarget);
                    }

                    Entry.AddAst(varTarget);
                }
                else
                {
                    Entry.AddAst(new VariableTarget(parameter.Name));
                }
            }

            return varTargets;
        }

        /// <summary>
        /// Used to analyze scriptbloct, functionmemberast or functiondefinitionast
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public void AnalyzeImpl(Ast ast)
        {
            if (!(ast is ScriptBlockAst || ast is FunctionMemberAst || ast is FunctionDefinitionAst))
            {
                return;
            }

            _variables = FindAllVariablesVisitor.Visit(ast);

            Dictionary<String, VariableTarget> varTargets = null;

            Init();

            if (ast is FunctionMemberAst || ast is FunctionDefinitionAst)
            {
                IEnumerable<ParameterAst> parameters = FindParameters(ast, ast.GetType());
                if (parameters != null)
                {
                    varTargets = ProcessParameters(parameters);
                }
            }
            else
            {
                ScriptBlockAst sbAst = ast as ScriptBlockAst;
                if (sbAst != null && sbAst.ParamBlock != null && sbAst.ParamBlock.Parameters != null)
                {
                    varTargets = ProcessParameters(sbAst.ParamBlock.Parameters);
                }
            }

            if (ast is FunctionMemberAst)
            {
                (ast as FunctionMemberAst).Body.Visit(this.Decorator);
            }
            else if (ast is FunctionDefinitionAst)
            {
                (ast as FunctionDefinitionAst).Body.Visit(this.Decorator);
            }
            else
            {
                ast.Visit(this.Decorator);
            }

            VariablesDictionary = Block.SparseSimpleConstants(_variables, Entry);

            // Update the type of variables in VariablesDictionary based on the param block
            foreach (var entry in VariablesDictionary)
            {
                var analysisDetails = entry.Value;
                if (analysisDetails.Type != typeof(Unreached))
                {
                    continue;
                }

                // This regex is used to extracts the variable name from entry.Key. The key is of the form [varname]s[so]e[eo]
                // where [varname] is name of variable [so] is the start offset number and [eo] is the end offset number
                var result = Regex.Match(entry.Key, "^(.+)s[0-9]+e[0-9]+$");
                if (result.Success && result.Groups.Count == 2)
                {
                    string varName = result.Groups[1].Value;
                    if (varTargets.ContainsKey(varName))
                    {
                        analysisDetails.Type = varTargets[varName].Type;
                    }
                }
            }
        }

        /// <summary>
        /// Return variableanalysisdetails for VarTarget.
        /// This function should only be called after Block.SparseSimpleConstants are called.
        /// </summary>
        /// <param name="varTarget"></param>
        /// <returns></returns>
        public VariableAnalysisDetails GetVariableAnalysis(VariableExpressionAst varTarget)
        {
            if (varTarget == null)
            {
                return null;
            }

            string key = AnalysisDictionaryKey(varTarget);

            if (!VariablesDictionary.ContainsKey(key))
            {
                return null;
            }

            return VariablesDictionary[key];
        }

        internal static string AnalysisDictionaryKey(VariableExpressionAst varExprAst)
        {
            if (varExprAst == null)
            {
                return String.Empty;
            }

            return String.Format(CultureInfo.CurrentCulture,
                "{0}s{1}e{2}",
                AssignmentTarget.GetUnaliasedVariableName(varExprAst.VariablePath),
                varExprAst.Extent.StartOffset,
                varExprAst.Extent.EndOffset
                );
        }

        /// <summary>
        /// Returns true if the variable is initialized.
        /// This function should only be called after SparseSimpleConstants are called.
        /// </summary>
        /// <param name="varTarget"></param>
        /// <returns></returns>
        public bool IsUninitialized(VariableExpressionAst varTarget)
        {
            if (varTarget == null)
            {
                return false;
            }

            var analysis = GetVariableAnalysis(varTarget);
            
            if (analysis == null)
            {
                return false;
            }

            return analysis.DefinedBlock == null
                && !SpecialVars.InitializedVariables.Contains(analysis.Name, StringComparer.OrdinalIgnoreCase)
                && !IsGlobalOrEnvironment(varTarget);
        }

        /// <summary>
        /// Returns true if the variable is not a global variable nor environment variable
        /// </summary>
        /// <param name="varTarget"></param>
        /// <returns></returns>
        public bool IsGlobalOrEnvironment(VariableExpressionAst varTarget)
        {
            if (varTarget != null)
            {
                return (varTarget.VariablePath.IsGlobal
                        || String.Equals(varTarget.VariablePath.DriveName, "env", StringComparison.OrdinalIgnoreCase));
            }
           
            return false;
            
        }

        /// <summary>
        /// Get assignment targets from expressionast
        /// </summary>
        /// <param name="expressionAst"></param>
        /// <returns></returns>
        internal static IEnumerable<ExpressionAst> GetAssignmentTargets(ExpressionAst expressionAst)
        {
            var parenExpr = expressionAst as ParenExpressionAst;
            if (parenExpr != null)
            {
                foreach (var e in GetAssignmentTargets(parenExpr.Pipeline.GetPureExpression()))
                {
                    yield return e;
                }
            }
            else
            {
                var arrayLiteral = expressionAst as ArrayLiteralAst;
                if (arrayLiteral != null)
                {
                    foreach (var e in arrayLiteral.Elements.SelectMany(GetAssignmentTargets))
                    {
                        yield return e;
                    }
                }
                else
                {
                    yield return expressionAst;
                }
            }
        }

        /// <summary>
        /// Visit assignment statement
        /// </summary>
        /// <param name="assignmentStatementAst"></param>
        /// <returns></returns>
        public override object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            if (assignmentStatementAst == null)
            {
                return null;
            }

            base.VisitAssignmentStatement(assignmentStatementAst);

            foreach (var assignTarget in GetAssignmentTargets(assignmentStatementAst.Left))
            {
                var leftAst = assignTarget;
                while (leftAst is AttributedExpressionAst)
                {
                    leftAst = ((AttributedExpressionAst)leftAst).Child;
                }

                if (leftAst is VariableExpressionAst)
                {
                    var varPath = ((VariableExpressionAst)leftAst).VariablePath;

                    if (_variables.ContainsKey(AssignmentTarget.GetUnaliasedVariableName(varPath)))
                    {
                        var details = _variables[AssignmentTarget.GetUnaliasedVariableName(varPath)];
                        details.AssignedBlocks.Add(Current);
                    }

                    Current.AddAst(new AssignmentTarget(assignmentStatementAst));
                }
                else
                {
                    // We skip things like $a.test = 3. In this case we will just test
                    // for variable $a
                    assignTarget.Visit(this.Decorator);
                }                
            }

            return null;
        }

        /// <summary>
        /// Visit variable expression ast
        /// </summary>
        /// <param name="variableExpressionAst"></param>
        /// <returns></returns>
        public override object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if (variableExpressionAst == null) return null;

            var varPath = variableExpressionAst.VariablePath;

            if (_variables.ContainsKey(AssignmentTarget.GetUnaliasedVariableName(varPath)))
            {
                var details = _variables[AssignmentTarget.GetUnaliasedVariableName(varPath)];
                Current.AddAst(new VariableTarget(variableExpressionAst));
                details.AssociatedAsts.Add(variableExpressionAst);
            }

            return base.VisitVariableExpression(variableExpressionAst);
        }
    }
}
