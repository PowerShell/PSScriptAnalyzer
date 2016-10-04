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

        public UseShouldProcessCorrectly()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            shouldProcessVertex = new Vertex("ShouldProcess", null);
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
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture,Strings.SourceName);
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

            bool callsShouldProcess = funcDigraph.IsConnected(v, shouldProcessVertex);
            if (DeclaresSupportsShouldProcess(fast))
            {
                if (!callsShouldProcess)
                {
                    return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.ShouldProcessErrorHasAttribute,
                            fast.Name),
                        ast.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        ast.Extent.File);
                }
            }
            else
            {
                if (callsShouldProcess)
                {
                    // check if upstream function declares SupportShouldProcess\
                    // if so, this might just be a helper function
                    // do not flag this case
                    if (UpstreamDeclaresShouldProcess(v))
                    {
                        return null;
                    }

                    return new DiagnosticRecord(
                         string.Format(
                             CultureInfo.CurrentCulture,
                             Strings.ShouldProcessErrorHasCmdlet,
                             fast.Name),
                            v.Ast.Extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName);
                }
            }

            return null;
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

        private CommandInfo GetCommandInfo(string cmdName)
        {
            try
            {
                using (var ps = System.Management.Automation.PowerShell.Create())
                {
                    var cmdInfo = ps.AddCommand("Get-Command")
                                    .AddArgument(cmdName)
                                    .Invoke<CommandInfo>()
                                    .FirstOrDefault();
                    return cmdInfo;
                }
            }
            catch (System.Management.Automation.CommandNotFoundException)
            {
                return null;
            }
        }

        private bool SupportsShouldProcess(string cmdName)
        {
            if (String.IsNullOrWhiteSpace(cmdName))
            {
                return false;
            }

            var cmdInfo = GetCommandInfo(cmdName);
            if (cmdInfo == null)
            {
                return false;
            }

            var cmdletInfo = cmdInfo as CmdletInfo;
            if (cmdletInfo == null)
            {
                // check if it is of functioninfo type
                var funcInfo = cmdInfo as FunctionInfo;
                if (funcInfo != null
                    && funcInfo.CmdletBinding
                    && funcInfo.ScriptBlock != null
                    && funcInfo.ScriptBlock.Attributes != null)
                {
                    foreach (var attr in funcInfo.ScriptBlock.Attributes)
                    {
                        var cmdletBindingAttr = attr as CmdletBindingAttribute;
                        if (cmdletBindingAttr != null)
                        {
                            return cmdletBindingAttr.SupportsShouldProcess;
                        }
                    }
                }

                return false;
            }

            var attributes = cmdletInfo.ImplementingType.GetTypeInfo().GetCustomAttributes(
                typeof(System.Management.Automation.CmdletCommonMetadataAttribute),
                true);

            foreach (var attr in attributes)
            {
                var cmdletAttribute = attr as System.Management.Automation.CmdletAttribute;
                if (cmdletAttribute != null)
                {
                    return cmdletAttribute.SupportsShouldProcess;
                }
            }

            return false;
        }

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
                funcDigraph.AddVertex(shouldProcessVertex);
                foreach(var v in commandsWithSupportShouldProcess)
                {
                    funcDigraph.AddEdge(v, shouldProcessVertex);
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
        public Ast Ast {get {return ast; }}

        private string name;
        private Ast ast;

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
            return name.ToLower().GetHashCode();
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
        public void AddVertex(Vertex name)
        {
            if (!digraph.ContainsVertex(name))
            {
                digraph.AddVertex(name);
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

            var functionVertex = new Vertex (ast.Name, ast);
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

            // if command is part of a binary module
            //   for now just check if (Get-Command <CommandName>).DLL end with dll extension
            // if so, check if it declares SupportsShouldProcess
            // if so, then assume it also calls ShouldProcess
            // because we do not have a way to analyze its definition
            // to actually verify it is indeed calling ShouddProcess

            // if (IsPartOfBinaryModule(cmdName, out cmdInfo))
            //   if (HasSupportShouldProcessAttribute(cmdInfo))
            //     AddEdge(cmdName, shouldProcessVertex)
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
            // and edge from <Expression>-><Member>. Even though <Expression> is not
            // necessarily a function, we do it because we are mainly interested in
            // finding connection between a function and ShouldProcess and this approach
            // prevents any unnecessary complexity.
            var exprVertex = new Vertex (expr, ast.Expression);
            var memberVertex = new Vertex (memberExprAst.Value, memberExprAst);
            AddVertex(exprVertex);
            AddVertex(memberVertex);
            AddEdge(exprVertex, memberVertex);
            if (IsWithinFunctionDefinition())
            {
                AddEdge(GetCurrentFunctionContext(), exprVertex);
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

        public int GetOutDegree(Vertex v)
        {
            return digraph.GetOutDegree(v);
        }
    }
}
