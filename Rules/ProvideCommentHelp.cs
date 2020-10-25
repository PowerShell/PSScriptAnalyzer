// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Text;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideCommentHelp: Analyzes ast to check that cmdlets have help.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class ProvideCommentHelp : ConfigurableRule
    {
        private CommentHelpPlacement placement;

        /// <summary>
        /// Construct an object of ProvideCommentHelp type.
        /// </summary>
        public ProvideCommentHelp() : base()
        {
            // Enable the rule by default
            Enable = true;
        }

        /// <summary>
        /// If enabled, throw violation only on functions/cmdlets that are exported using
        /// the "Export-ModuleMember" cmdlet.
        ///
        /// Default value is true.
        ///</summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool ExportedOnly { get; protected set; }

        /// <summary>
        /// If enabled, returns comment help in block comment style, i.e., `<#...#>`. Otherwise returns
        /// comment help in line comment style, i.e., each comment line starts with `#`.
        ///
        /// Default value is true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool BlockComment { get; protected set; }

        /// <summary>
        /// If enabled, returns comment help in vscode snippet format.
        ///
        /// Default value is false.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool VSCodeSnippetCorrection { get; protected set; }

        /// <summary>
        /// Represents the position of comment help with respect to the function definition.
        ///
        /// Possible values are: `before`, `begin` and `end`. If any invalid value is given, the
        /// property defaults to `before`.
        ///
        /// `before` means the help is placed before the function definition.
        /// `begin` means the help is placed at the begining of the function definition body.
        /// `end` means the help is places the end of the function definition body.
        ///</summary>
        [ConfigurableRuleProperty(defaultValue: "before")]
        public string Placement
        {
            get
            {
                return placement.ToString();
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value) ||
                    !Enum.TryParse<CommentHelpPlacement>(value, true, out placement))
                {
                    placement = CommentHelpPlacement.Before;
                }
            }
        }

        private enum CommentHelpPlacement { Before, Begin, End };

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that cmdlets have help.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            var exportedFunctions = Helper.Instance.GetExportedFunction(ast);
            var violationFinder = new ViolationFinder(exportedFunctions, ExportedOnly);
            ast.Visit(violationFinder);
            foreach (var functionDefinitionAst in violationFinder.FunctionDefinitionAsts)
            {
                yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpError, functionDefinitionAst.Name),
                        Helper.Instance.GetScriptExtentForFunctionName(functionDefinitionAst),
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrection(ast, functionDefinitionAst).ToList());
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ProvideCommentHelpName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        // TODO replace with extension version
        private static IEnumerable<string> GetLines(string text)
        {
            return text.Split('\n').Select(l => l.Trim('\r'));
        }

        private DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Information;
        }

        private IEnumerable<CorrectionExtent> GetCorrection(Ast ast, FunctionDefinitionAst funcDefnAst)
        {
            var helpBuilder = new CommentHelpBuilder();

            // todo replace with an extension version
            var paramAsts = (funcDefnAst.Parameters ?? funcDefnAst.Body.ParamBlock?.Parameters)
                                ?? Enumerable.Empty<ParameterAst>();
            foreach (var paramAst in paramAsts)
            {
                helpBuilder.AddParameter(paramAst.Name.VariablePath.UserPath);
            }

            int startLine, endLine, startColumn, endColumn;
            GetCorrectionPosition(funcDefnAst, out startLine, out endLine, out startColumn, out endColumn);
            yield return new CorrectionExtent(
                startLine,
                endLine,
                startColumn,
                endColumn,
                GetCorrectionText(
                    helpBuilder.GetCommentHelp(BlockComment, VSCodeSnippetCorrection),
                    ast,
                    funcDefnAst),
                funcDefnAst.Extent.File);
        }

        private string GetCorrectionText(string correction, Ast ast, FunctionDefinitionAst funcDefnAst)
        {
            var indentationString = String.Empty;
            if (funcDefnAst.Extent.StartColumnNumber > 1)
            {
                indentationString = GetLines(ast.Extent.Text)
                    .ElementAt(funcDefnAst.Extent.StartLineNumber - 1)
                    .Substring(0, funcDefnAst.Extent.StartColumnNumber - 1);
                correction = String.Join(
                        Environment.NewLine,
                        GetLines(correction).Select(l => indentationString + l));
            }

            switch (placement)
            {
                case CommentHelpPlacement.Begin:
                    return $"{{{Environment.NewLine}{correction}{Environment.NewLine}";

                case CommentHelpPlacement.End:
                    return $"{Environment.NewLine}{correction}{Environment.NewLine}{indentationString}";

                default: // CommentHelpPlacement.Before
                    return $"{correction}{Environment.NewLine}";
            }
        }

        private void GetCorrectionPosition(
            FunctionDefinitionAst funcDefnAst,
            out int startLine,
            out int endLine,
            out int startColumn,
            out int endColumn)
        {
            // the better thing to do is get the line/column from corresponding tokens
            switch (placement)
            {
                case CommentHelpPlacement.Begin:
                    startLine = funcDefnAst.Body.Extent.StartLineNumber;
                    endLine = startLine;
                    startColumn = funcDefnAst.Body.Extent.StartColumnNumber;
                    endColumn = startColumn + 1;
                    break;

                case CommentHelpPlacement.End:
                    startLine = funcDefnAst.Body.Extent.EndLineNumber;
                    endLine = startLine;
                    startColumn = funcDefnAst.Body.Extent.EndColumnNumber - 1;
                    endColumn = startColumn;
                    break;

                default: // CommentHelpPlacement.Before
                    startLine = funcDefnAst.Extent.StartLineNumber;
                    endLine = startLine;
                    startColumn = 1;
                    endColumn = startColumn;
                    break;
            }
        }

        private class ViolationFinder : AstVisitor
        {
            private HashSet<string> functionAllowList;
            private List<FunctionDefinitionAst> functionDefinitionAsts;
            private bool useFunctionAllowList;

            public ViolationFinder()
            {
                functionAllowList = new HashSet<string>();
                functionDefinitionAsts = new List<FunctionDefinitionAst>();
            }

            public ViolationFinder(HashSet<string> exportedFunctions) : this()
            {
                if (exportedFunctions == null)
                {
                    throw new ArgumentNullException(nameof(exportedFunctions));
                }

                this.functionAllowList = exportedFunctions;
            }

            public ViolationFinder(HashSet<string> exportedFunctions, bool exportedOnly) : this(exportedFunctions)
            {
                this.useFunctionAllowList = exportedOnly;
            }

            public IEnumerable<FunctionDefinitionAst> FunctionDefinitionAsts
            {
                get
                {
                    return functionDefinitionAsts;
                }
            }

            public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst funcAst)
            {
                if ((!useFunctionAllowList || functionAllowList.Contains(funcAst.Name))
                    && funcAst.GetHelpContent() == null)
                {
                    functionDefinitionAsts.Add(funcAst);
                }

                return AstVisitAction.Continue;
            }
        }

        private class CommentHelpBuilder
        {
            private CommentHelpNode synopsis;
            private CommentHelpNode description;
            private List<CommentHelpNode> parameters;
            private CommentHelpNode example;
            private CommentHelpNode notes;

            public CommentHelpBuilder()
            {
                synopsis = new CommentHelpNode("Synopsis", "Short description");
                description = new CommentHelpNode("Description", "Long description");
                example = new CommentHelpNode("Example", "An example");
                parameters = new List<CommentHelpNode>();
                notes = new CommentHelpNode("Notes", "General notes");
            }

            public void AddParameter(string paramName)
            {
                parameters.Add(new ParameterHelpNode(paramName, "Parameter description"));
            }

            public string GetCommentHelp(bool blockComment, bool snippet)
            {
                var sb = new StringBuilder();
                var helpContent = snippet ? this.ToSnippetString() : this.ToString();
                if (blockComment)
                {
                    sb.AppendLine("<#");
                    sb.AppendLine(helpContent);
                    sb.Append("#>");
                }
                else
                {
                    var boundaryString = new String('#', 30);
                    sb.AppendLine(boundaryString);
                    var lines = GetLines(helpContent);
                    foreach (var line in lines)
                    {
                        sb.Append("#");
                        sb.AppendLine(line);
                    }

                    sb.Append(boundaryString);
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                return ToString(false);
            }

            // todo remove code duplication
            public string ToSnippetString()
            {
                return ToString(true);
            }

            private string ToString(bool snippetify)
            {
                var sb = new StringBuilder();
                var counter = new Counter(snippetify ? (int?)1 : null);
                sb.AppendLine(synopsis.ToString(counter.Next())).AppendLine();
                sb.AppendLine(description.ToString(counter.Next())).AppendLine();
                foreach (var parameter in parameters)
                {
                    sb.AppendLine(parameter.ToString(counter.Next())).AppendLine();
                }

                sb.AppendLine(example.ToString(counter.Next())).AppendLine();
                sb.Append(notes.ToString(counter.Next()));
                return sb.ToString();
            }

            private class Counter
            {
                int? count;

                public Counter(int? start)
                {
                    count = start;
                }

                public int? Next()
                {
                    return count.HasValue ? count++ : null;
                }
            }

            private class CommentHelpNode
            {
                public CommentHelpNode(string nodeName, string description)
                {
                    Name = nodeName;
                    Description = description;
                }

                public string Name { get; }
                public string Description { get; set; }

                public override string ToString()
                {
                    return ToString(null);
                }

                public virtual string ToString(int? tabStop)
                {
                    var sb = new StringBuilder();
                    sb.Append(".").AppendLine(Name.ToUpperInvariant()); // ToUpperInvariant is important to also work with turkish culture, see https://github.com/PowerShell/PSScriptAnalyzer/issues/1095
                    if (!String.IsNullOrWhiteSpace(Description))
                    {
                        sb.Append(Snippetify(tabStop, Description));
                    }

                    return sb.ToString();
                }

                protected string Snippetify(int? tabStop, string defaultValue)
                {
                    return tabStop == null ? defaultValue : $"${{{tabStop}:{defaultValue}}}";
                }
            }

            private class ParameterHelpNode : CommentHelpNode
            {
                public ParameterHelpNode(string parameterName, string parameterDescription)
                    : base("Parameter", parameterDescription)
                {
                    ParameterName = parameterName;
                }

                public string ParameterName { get; }

                public override string ToString()
                {
                    return ToString(null);
                }

                public override string ToString(int? tabStop)
                {
                    // todo replace with String.GetLines() extension once it is available
                    var lines = base.ToString(tabStop).Split('\n').Select(l => l.Trim('\r')).ToArray();
                    lines[0] = $"{lines[0]} {ParameterName}";
                    return String.Join(Environment.NewLine, lines);
                }
            }
        }
    }
}
