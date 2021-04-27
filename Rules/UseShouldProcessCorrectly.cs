// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseShouldProcessCorrectly: Analyzes the ast to check that if the ShouldProcess attribute is present, the function calls ShouldProcess and vice versa.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseShouldProcessCorrectly : IScriptRule
    {
        private Ast ast;
        private string fileName;
        private FunctionReferenceDigraph funcDigraph;
        private List<DiagnosticRecord> diagnosticRecords;
        private readonly Vertex shouldProcessVertex;
        private readonly Vertex implicitShouldProcessVertex;

        public UseShouldProcessCorrectly()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            shouldProcessVertex = new Vertex("ShouldProcess", null);
            implicitShouldProcessVertex = new Vertex("implicitShouldProcessVertex", null);
        }

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that if the ShouldProcess attribute is present, the function calls ShouldProcess and vice versa.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            diagnosticRecords.Clear();
            this.ast = ast;
            this.fileName = fileName;
            funcDigraph = new FunctionReferenceDigraph();
            ast.Visit(funcDigraph);
            CheckForSupportShouldProcess();
            FindViolations();
            foreach (var dr in diagnosticRecords)
            {
                yield return dr;
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ShouldProcessName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ShouldProcessCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture,Strings.ShouldProcessDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        private DiagnosticSeverity GetDianosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Find the violations in the current AST
        /// </summary>
        private void FindViolations()
        {
            foreach (var v in funcDigraph.GetVertices())
            {
                var dr = GetViolation(v);
                if (dr != null)
                {
                    diagnosticRecords.Add(dr);
                }
            }
        }

        /// <summary>
        /// Get violation for a given function definition node
        /// </summary>
        /// <param name="v">Graph vertex, must be non-null</param>
        /// <returns>An instance of DiagnosticRecord if it find violation, otherwise null</returns>
        private DiagnosticRecord GetViolation(Vertex v)
        {
            FunctionDefinitionAst fast = v.Ast as FunctionDefinitionAst;
            if (fast == null)
            {
                return null;
            }

            if (DeclaresSupportsShouldProcess(fast))
            {
                bool callsShouldProcess = funcDigraph.IsConnected(v, shouldProcessVertex);
                bool callsCommandWithShouldProcess = funcDigraph.IsConnected(v, implicitShouldProcessVertex);
                if (!callsShouldProcess
                    && !callsCommandWithShouldProcess)
                {
                    return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.ShouldProcessErrorHasAttribute,
                            fast.Name),
                        Helper.Instance.GetShouldProcessAttributeAst(fast.Body.ParamBlock.Attributes).Extent,
                        GetName(),
                        GetDianosticSeverity(),
                        ast.Extent.File);
                }
            }
            else
            {
                if (callsShouldProcessDirectly(v))
                {
                    // check if upstream function declares SupportShouldProcess
                    // if so, this might just be a helper function
                    // do not flag this case
                    if (v.IsNestedFunctionDefinition && UpstreamDeclaresShouldProcess(v))
                    {
                        return null;
                    }

                    return new DiagnosticRecord(
                         string.Format(
                             CultureInfo.CurrentCulture,
                             Strings.ShouldProcessErrorHasCmdlet,
                             fast.Name),
                            GetShouldProcessCallExtent(fast),
                            GetName(),
                            GetDianosticSeverity(),
                            fileName);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the extent of ShouldProcess call
        /// </summary>
        private static IScriptExtent GetShouldProcessCallExtent(FunctionDefinitionAst functionDefinitionAst)
        {
            var invokeMemberExpressionAstFound = functionDefinitionAst.Find(IsShouldProcessCall, true);
            if (invokeMemberExpressionAstFound == null)
            {
                return functionDefinitionAst.Extent;
            }

            return (invokeMemberExpressionAstFound as InvokeMemberExpressionAst).Member.Extent;
        }

        /// <summary>
        /// Returns true if ast if of the form $PSCmdlet.PSShouldProcess()
        /// </summary>
        private static bool IsShouldProcessCall(Ast ast)
        {
            var invokeMemberExpressionAst = ast as InvokeMemberExpressionAst;
            if (invokeMemberExpressionAst == null)
            {
                return false;
            }

            var memberExprAst = invokeMemberExpressionAst.Member as StringConstantExpressionAst;
            if (memberExprAst == null)
            {
                return false;
            }

            if ("ShouldProcess".Equals(memberExprAst.Value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool callsShouldProcessDirectly(Vertex vertex)
        {
            return funcDigraph.GetNeighbors(vertex).Contains(shouldProcessVertex);
        }

        /// <summary>
        /// Checks if an upstream function declares SupportsShouldProcess
        /// </summary>
        /// <param name="v">Graph vertex, must be non-null</param>
        /// <returns>true if an upstream function declares SupportsShouldProcess, otherwise false</returns>
        private bool UpstreamDeclaresShouldProcess(Vertex v)
        {
            foreach (var vertex in funcDigraph.GetVertices())
            {
                if (v.Equals(vertex))
                {
                    continue;
                }

                var fast = vertex.Ast as FunctionDefinitionAst;
                if (fast == null)
                {
                    continue;
                }

                if (DeclaresSupportsShouldProcess(fast)
                    && funcDigraph.IsConnected(vertex, v))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a function declares SupportShouldProcess attribute
        /// </summary>
        /// <param name="ast">A non-null instance of FunctionDefinitionAst type</param>
        /// <returns>True if the given function declares SupportShouldProcess, otherwise null</returns>
        private bool DeclaresSupportsShouldProcess(FunctionDefinitionAst ast)
        {
            if (ast.Body.ParamBlock == null
                || ast.Body.ParamBlock.Attributes == null)
            {
                return false;
            }

            var shouldProcessAttribute = Helper.Instance.GetShouldProcessAttributeAst(ast.Body.ParamBlock.Attributes);
            if (shouldProcessAttribute == null)
            {
                return false;
            }

            return Helper.Instance.GetNamedArgumentAttributeValue(shouldProcessAttribute);
        }

        /// <summary>
        /// Checks if the given command supports ShouldProcess
        /// </summary>
        /// <returns>False if input is null. If the input command has declares SupportsShouldProcess attribute, returns true</returns>
        private bool SupportsShouldProcess(string cmdName)
        {
            if (String.IsNullOrWhiteSpace(cmdName))
            {
                return false;
            }

            CommandInfo cmdInfo = Helper.Instance.GetCommandInfo(cmdName);
            if (cmdInfo == null)
            {
                return false;
            }

            switch (cmdInfo)
            {
                case CmdletInfo cmdletInfo:
                    return cmdletInfo.ImplementingType.GetCustomAttribute<CmdletAttribute>(inherit: true).SupportsShouldProcess;

                case FunctionInfo functionInfo:
                    try
                    {
                        if (!functionInfo.CmdletBinding
                            || functionInfo.ScriptBlock?.Attributes == null)
                        {
                            break;
                        }

                        foreach (CmdletBindingAttribute cmdletBindingAttribute in functionInfo.ScriptBlock.Attributes.OfType<CmdletBindingAttribute>())
                        {
                            return cmdletBindingAttribute.SupportsShouldProcess;
                        }
                    }
                    catch
                    {
                        // functionInfo.ScriptBlock.Attributes may throw if it cannot resolve an attribute type
                        // Instead we fall back to AST analysis
                        // See: https://github.com/PowerShell/PSScriptAnalyzer/issues/1217
                        if (TryGetShouldProcessValueFromAst(functionInfo, out bool hasShouldProcessSet))
                        {
                            return hasShouldProcessSet;
                        }
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Attempt to find whether a function has SupportsShouldProcess set based on its AST.
        /// </summary>
        /// <param name="functionInfo">The function info object referring to the function.</param>
        /// <param name="hasShouldProcessSet">True if SupportsShouldProcess is set, false if not. Value is not valid if this method returns false.</param>
        /// <returns>True if a value for SupportsShouldProcess was found, false otherwise.</returns>
        private bool TryGetShouldProcessValueFromAst(FunctionInfo functionInfo, out bool hasShouldProcessSet)
        {
            // Get the body of the function
            ScriptBlockAst functionBodyAst = (ScriptBlockAst)functionInfo.ScriptBlock.Ast.Find(ast => ast is ScriptBlockAst, searchNestedScriptBlocks: true);

            // Go through attributes on the parameter block, since this is where [CmdletBinding()] will be
            foreach (AttributeAst attributeAst in functionBodyAst.ParamBlock.Attributes)
            {
                // We're looking for [CmdletBinding()]
                if (!attributeAst.TypeName.FullName.Equals("CmdletBinding", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (NamedAttributeArgumentAst namedArgumentAst in attributeAst.NamedArguments)
                {
                    // We want [CmdletBinding(SupportsShouldProcess)]
                    if (!namedArgumentAst.ArgumentName.Equals("SupportsShouldProcess", StringComparison.OrdinalIgnoreCase))

                    {
                        continue;
                    }

                    // [CmdletBinding(SupportsShouldProcess)] is the same as [CmdletBinding(SupportsShouldProcess = $true)]
                    if (namedArgumentAst.ExpressionOmitted)
                    {
                        hasShouldProcessSet = true;
                        return true;
                    }

                    // Otherwise try to get the value assigned to the parameter, and assume false if value cannot be determined
                    try
                    {
                        hasShouldProcessSet = LanguagePrimitives.IsTrue(
                            Helper.GetSafeValueFromExpressionAst(
                                namedArgumentAst.Argument));
                    }
                    catch
                    {
                        hasShouldProcessSet = false;
                    }

                    return true;
                }
            }

            hasShouldProcessSet = false;
            return false;
        }

        /// <summary>
        /// Add a ShouldProcess edge from a command vertex if the command supports ShouldProcess
        /// </summary>
        private void CheckForSupportShouldProcess()
        {
            var commandsWithSupportShouldProcess = new List<Vertex>();

            // for all the vertices without any neighbors check if they support shouldprocess
            foreach (var v in funcDigraph.GetVertices())
            {
                if (funcDigraph.GetOutDegree(v) == 0)
                {
                    if (SupportsShouldProcess(v.Name))
                    {
                        commandsWithSupportShouldProcess.Add(v);
                    }
                }
            }

            if (commandsWithSupportShouldProcess.Count > 0)
            {
                funcDigraph.AddVertex(implicitShouldProcessVertex);
                foreach(var v in commandsWithSupportShouldProcess)
                {
                    funcDigraph.AddEdge(v, implicitShouldProcessVertex);
                }
            }
        }
    }

    /// <summary>
    /// Class to represent a vertex in a function call graph
    /// </summary>
    class Vertex
    {
        public string Name {get { return name;}}
        public Ast Ast
        {
            get
            {
                return ast;
            }
            set
            {
                ast = value;
            }
        }

        public bool IsNestedFunctionDefinition {get {return isNestedFunctionDefinition;}}

        private string name;
        private Ast ast;
        private bool isNestedFunctionDefinition;

        public Vertex()
        {
            name = String.Empty;
        }

        public Vertex (string name, Ast ast)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.name = name;
            this.ast = ast;
        }

        public Vertex (String name, Ast ast, bool isNestedFunctionDefinition)
            : this(name, ast)
            {
            this.isNestedFunctionDefinition = isNestedFunctionDefinition;
        }

        /// <summary>
        /// Returns string representation of a Vertex instance
        /// </summary>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        /// Compares two instances of Vertex class to check for equality
        /// </summary>
        public override bool Equals(Object other)
        {
            var otherVertex = other as Vertex;
            if (otherVertex == null)
            {
                return false;
            }

            if (name.Equals(otherVertex.name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the Hash code of the given Vertex instance
        /// </summary>
        public override int GetHashCode()
        {
            return name.ToLowerInvariant().GetHashCode();
        }
    }

    /// <summary>
    /// Class to encapsulate a function call graph and related actions
    /// </summary>
    class FunctionReferenceDigraph : AstVisitor
    {
        private Digraph<Vertex> digraph;

        private Stack<Vertex> functionVisitStack;

        /// <summary>
        /// Checks if the AST being visited is in an instance FunctionDefinitionAst type
        /// </summary>
        private bool IsWithinFunctionDefinition()
        {
            return functionVisitStack.Count > 0;
        }

        /// <summary>
        /// Returns the function vertex whose children are being currently visited
        /// </summary>
        private Vertex GetCurrentFunctionContext()
        {
            return functionVisitStack.Peek();
        }

        /// <summary>
        /// Return the constructed digraph
        /// </summary>
        public Digraph<Vertex> GetDigraph()
        {
            return digraph;
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        public FunctionReferenceDigraph()
        {
            digraph = new Digraph<Vertex>();
            functionVisitStack = new Stack<Vertex>();
        }

        /// <summary>
        /// Add a vertex to the graph
        /// </summary>
        public void AddVertex(Vertex vertex)
        {
            bool containsVertex = false;

            // if the graph contains a vertex with name equal to that
            // of the input vertex, then update the vertex's ast if the
            // input vertex's ast is of FunctionDefinitionAst type
            foreach(Vertex v in digraph.GetVertices())
            {
                if (v.Equals(vertex))
                {
                    containsVertex = true;
                    if (vertex.Ast != null
                        && vertex.Ast is FunctionDefinitionAst)
                    {
                        v.Ast = vertex.Ast;
                    }
                    break;
                }
            }

            if (!containsVertex)
            {
                digraph.AddVertex(vertex);
            }
        }

        /// <summary>
        /// Add an edge from a vertex to another vertex
        /// </summary>
        /// <param name="fromV">start of the edge</param>
        /// <param name="toV">end of the edge</param>
        public void AddEdge(Vertex fromV, Vertex toV)
        {
            if (!digraph.GetNeighbors(fromV).Contains(toV))
            {
                digraph.AddEdge(fromV, toV);
            }
        }

        /// <summary>
        /// Add a function to the graph; create a function context; and visit the function body
        /// </summary>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst ast)
        {
            if (ast == null)
            {
                return AstVisitAction.SkipChildren;
            }

            var functionVertex = new Vertex(ast.Name, ast, IsWithinFunctionDefinition());
            functionVisitStack.Push(functionVertex);
            AddVertex(functionVertex);
            ast.Body.Visit(this);
            functionVisitStack.Pop();
            return AstVisitAction.SkipChildren;
        }

        /// <summary>
        /// Add a command to the graph and if within a function definition, add an edge from the calling function to the command
        /// </summary>
        public override AstVisitAction VisitCommand(CommandAst ast)
        {
            if (ast == null)
            {
                return AstVisitAction.SkipChildren;
            }

            var cmdName = ast.GetCommandName();
            if (cmdName == null)
            {
                return AstVisitAction.Continue;
            }

            var vertex = new Vertex (cmdName, ast);
            AddVertex(vertex);
            if (IsWithinFunctionDefinition())
            {
                AddEdge(GetCurrentFunctionContext(), vertex);
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Add a member to the graph and if within a function definition, add an edge from the function to the member
        /// </summary>
        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst ast)
        {
            if (ast == null)
            {
                return AstVisitAction.SkipChildren;
            }

            var expr = ast.Expression.Extent.Text;
            var memberExprAst = ast.Member as StringConstantExpressionAst;
            if (memberExprAst == null)
            {
                return AstVisitAction.Continue;
            }

            var member = memberExprAst.Value;
            if (string.IsNullOrWhiteSpace(member))
            {
                return AstVisitAction.Continue;
            }

            // Suppose we find <Expression>.<Member>, we split it up and create
            // and edge only to <Member>. Even though <Expression> is not
            // necessarily a function, we do it because we are mainly interested in
            // finding connection between a function and ShouldProcess and this approach
            // prevents any unnecessary complexity.
            var memberVertex = new Vertex (memberExprAst.Value, memberExprAst);
            AddVertex(memberVertex);
            if (IsWithinFunctionDefinition())
            {
                AddEdge(GetCurrentFunctionContext(), memberVertex);
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Return the vertices in the graph
        /// </summary>
        public IEnumerable<Vertex> GetVertices()
        {
            return digraph.GetVertices();
        }

        /// <summary>
        /// Check if two vertices are connected
        /// </summary>
        /// <param name="vertex">Origin vertxx</param>
        /// <param name="shouldVertex">Destination vertex</param>
        /// <returns></returns>
        public bool IsConnected(Vertex vertex, Vertex shouldVertex)
        {
            if (digraph.ContainsVertex(vertex)
                && digraph.ContainsVertex(shouldVertex))
            {
                return digraph.IsConnected(vertex, shouldVertex);
            }
            return false;
        }

        /// <summary>
        /// Get the number of edges out of the given vertex
        /// </summary>
        public int GetOutDegree(Vertex v)
        {
            return digraph.GetOutDegree(v);
        }

        public IEnumerable<Vertex> GetNeighbors(Vertex v)
        {
            return digraph.GetNeighbors(v);
        }
    }
}
