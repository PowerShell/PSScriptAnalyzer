// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseConsistentIndentation: Checks if indentation is consistent throughout the source file.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConsistentIndentation : ConfigurableRule
    {
        /// <summary>
        /// The indentation size in number of space characters.
        ///
        /// Default value if 4.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: 4)]
        public int IndentationSize { get; protected set; }


        // Cannot name to IndentationKind due to the enum type of the same name.
        /// <summary>
        /// Represents the kind of indentation to be used.
        ///
        /// Possible values are: `space`, `tab`. If any invalid value is given, the
        /// property defaults to `space`.
        ///
        /// `space` means `IndentationSize` number of `space` characters are used to provide one level of indentation.
        /// `tab` means a tab character, `\t`.
        ///</summary>
        [ConfigurableRuleProperty(defaultValue: "space")]
        public string Kind
        {
            get
            {
                return indentationKind.ToString();
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value) ||
                    !Enum.TryParse<IndentationKind>(value, true, out indentationKind))
                {
                    indentationKind = IndentationKind.Space;
                }
            }
        }


        [ConfigurableRuleProperty(defaultValue: "IncreaseIndentationForFirstPipeline")]
        public string PipelineIndentation
        {
            get
            {
                return pipelineIndentationStyle.ToString();
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value) ||
                    !Enum.TryParse(value, true, out pipelineIndentationStyle))
                {
                    pipelineIndentationStyle = PipelineIndentationStyle.IncreaseIndentationForFirstPipeline;
                }
            }
        }

        private bool insertSpaces;
        private char indentationChar;
        private int indentationLevelMultiplier;

        // TODO Enable auto when the rule is able to detect indentation
        private enum IndentationKind {
            Space,
            Tab,
            // Auto
        };

        private enum PipelineIndentationStyle
        {
            IncreaseIndentationForFirstPipeline,
            IncreaseIndentationAfterEveryPipeline,
            NoIndentation
        }

        // TODO make this configurable
        private IndentationKind indentationKind = IndentationKind.Space;

        private PipelineIndentationStyle pipelineIndentationStyle = PipelineIndentationStyle.IncreaseIndentationForFirstPipeline;

        /// <summary>
        /// Analyzes the given ast to find violations.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            // we add this switch because there is no clean way
            // to disable the rule by default
            if (!Enable)
            {
                return Enumerable.Empty<DiagnosticRecord>();
            }

            // It is more efficient to initialize these fields in ConfigurRule method
            // but when the rule will enable `Auto` IndentationKind, we will anyways need to move
            // the setting of these variables back here after the rule detects the indentation kind for
            // each invocation.
            insertSpaces = indentationKind == IndentationKind.Space;
            indentationChar = insertSpaces ? ' ' : '\t';
            indentationLevelMultiplier = insertSpaces ? IndentationSize : 1;

            var tokens = Helper.Instance.Tokens;
            var diagnosticRecords = new List<DiagnosticRecord>();
            var indentationLevel = 0;
            var onNewLine = true;
            var pipelineAsts = ast.FindAll(testAst => testAst is PipelineAst && (testAst as PipelineAst).PipelineElements.Count > 1, true);
            for (int k = 0; k < tokens.Length; k++)
            {
                var token = tokens[k];

                if (token.Kind == TokenKind.EndOfInput)
                {
                    break;
                }

                switch (token.Kind)
                {
                    case TokenKind.AtCurly:
                    case TokenKind.AtParen:
                    case TokenKind.LParen:
                    case TokenKind.LCurly:
                    case TokenKind.DollarParen:
                        AddViolation(token, indentationLevel++, diagnosticRecords, ref onNewLine);
                        break;

                    case TokenKind.Pipe:
                        bool pipelineIsFollowedByNewlineOrLineContinuation = k < tokens.Length - 1 && k > 0 &&
                              (tokens[k + 1].Kind == TokenKind.NewLine || tokens[k + 1].Kind == TokenKind.LineContinuation);
                        if (!pipelineIsFollowedByNewlineOrLineContinuation)
                        {
                            break;
                        }
                        if (pipelineIndentationStyle == PipelineIndentationStyle.IncreaseIndentationAfterEveryPipeline)
                        {
                            AddViolation(token, indentationLevel++, diagnosticRecords, ref onNewLine);
                            break;
                        }
                        if (pipelineIndentationStyle == PipelineIndentationStyle.IncreaseIndentationForFirstPipeline)
                        {
                            bool isFirstPipeInPipeline = pipelineAsts.Any(pipelineAst => PositionIsEqual(((PipelineAst)pipelineAst).PipelineElements[0].Extent.EndScriptPosition, tokens[k - 1].Extent.EndScriptPosition));
                            if (isFirstPipeInPipeline)
                            {
                                AddViolation(token, indentationLevel++, diagnosticRecords, ref onNewLine);
                            }
                        }
                        break;

                    case TokenKind.RParen:
                    case TokenKind.RCurly:
                        indentationLevel = ClipNegative(indentationLevel - 1);
                        AddViolation(token, indentationLevel, diagnosticRecords, ref onNewLine);
                        break;

                    case TokenKind.NewLine:
                    case TokenKind.LineContinuation:
                        onNewLine = true;
                        break;

                    default:
                        // we do not want to make a call for every token, hence
                        // we add this redundant check
                        if (onNewLine)
                        {
                            var tempIndentationLevel = indentationLevel;

                            // Check if the preceding character is an escape character
                            if (k > 0 && tokens[k - 1].Kind == TokenKind.LineContinuation)
                            {
                                ++tempIndentationLevel;
                            }
                            else
                            {
                                // Ignore comments
                                // Since the previous token is a newline token we start
                                // looking for comments at the token before the newline token.
                                int j = k - 2;
                                while (j > 0 && tokens[j].Kind == TokenKind.Comment)
                                {
                                    --j;
                                }
                            }

                            var lineHasPipelineBeforeToken = tokens.Any(oneToken =>
                                oneToken.Kind == TokenKind.Pipe &&
                                oneToken.Extent.StartLineNumber == token.Extent.StartLineNumber &&
                                oneToken.Extent.StartColumnNumber < token.Extent.StartColumnNumber);

                            AddViolation(token, tempIndentationLevel, diagnosticRecords, ref onNewLine, lineHasPipelineBeforeToken);
                        }
                        break;
                }

                // Check if the current token matches the end of a PipelineAst
                var matchingPipeLineAstEnd = pipelineAsts.FirstOrDefault(pipelineAst =>
                        PositionIsEqual(pipelineAst.Extent.EndScriptPosition, token.Extent.EndScriptPosition)) as PipelineAst;

                if (matchingPipeLineAstEnd == null)
                {
                    continue;
                }

                bool pipelineSpansOnlyOneLine = matchingPipeLineAstEnd.Extent.StartLineNumber == matchingPipeLineAstEnd.Extent.EndLineNumber;
                if (pipelineSpansOnlyOneLine)
                {
                    continue;
                }

                if (pipelineIndentationStyle == PipelineIndentationStyle.IncreaseIndentationForFirstPipeline)
                {
                    indentationLevel = ClipNegative(indentationLevel - 1);
                }
                else if (pipelineIndentationStyle == PipelineIndentationStyle.IncreaseIndentationAfterEveryPipeline)
                {
                    indentationLevel = ClipNegative(indentationLevel - (matchingPipeLineAstEnd.PipelineElements.Count - 1));
                }
            }

            return diagnosticRecords;
        }

        private static bool PositionIsEqual(IScriptPosition position1, IScriptPosition position2)
        {
            return position1.ColumnNumber == position2.ColumnNumber &&
                   position1.LineNumber == position2.LineNumber &&
                   position1.File == position2.File;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentIndentationCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentIndentationDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseConsistentIndentationName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        private void AddViolation(
            Token token,
            int expectedIndentationLevel,
            List<DiagnosticRecord> diagnosticRecords,
            ref bool onNewLine,
            bool lineHasPipelineBeforeToken = false)
        {
            if (onNewLine)
            {
                onNewLine = false;
                if (token.Extent.StartColumnNumber - 1 != GetIndentation(expectedIndentationLevel))
                {
                    var fileName = token.Extent.File;
                    var extent = token.Extent;
                    var violationExtent = extent = new ScriptExtent(
                        new ScriptPosition(
                            fileName,
                            extent.StartLineNumber,
                            1, // first column in the line
                            extent.StartScriptPosition.Line),
                        new ScriptPosition(
                            fileName,
                            extent.StartLineNumber,
                            extent.StartColumnNumber,
                            extent.StartScriptPosition.Line));
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.UseConsistentIndentationError),
                            violationExtent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName,
                            null,
                            GetSuggestedCorrections(token, expectedIndentationLevel, lineHasPipelineBeforeToken)));
                }
            }
        }

        private List<CorrectionExtent> GetSuggestedCorrections(
            Token token,
            int indentationLevel,
            bool lineHasPipelineBeforeToken = false)
        {
            // TODO Add another constructor for correction extent that takes extent
            // TODO handle param block
            // TODO handle multiline commands

            var corrections = new List<CorrectionExtent>();
            var optionalPipeline = lineHasPipelineBeforeToken ? "| " : string.Empty;
            corrections.Add(new CorrectionExtent(
                token.Extent.StartLineNumber,
                token.Extent.EndLineNumber,
                1,
                token.Extent.EndColumnNumber,
                GetIndentationString(indentationLevel) + optionalPipeline + token.Extent.Text,
                token.Extent.File));
            return corrections;
        }

        private static int ClipNegative(int x)
        {
            return x > 0 ? x : 0;
        }

        private int GetIndentationColumnNumber(int indentationLevel)
        {
            return GetIndentation(indentationLevel) + 1;
        }

        private int GetIndentation(int indentationLevel)
        {
            // todo if condition can be evaluated during rule configuration
            return indentationLevel * indentationLevelMultiplier;
        }

        private string GetIndentationString(int indentationLevel)
        {
            return new string(indentationChar, GetIndentation(indentationLevel));
        }
    }
}
