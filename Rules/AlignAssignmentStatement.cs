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
    /// AlignAssignmentStatement: Checks if consecutive assignment statements are aligned.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AlignAssignmentStatement : ConfigurableRule
    {

        /// <summary>
        /// Check the key value pairs of a hashtable, including DSC configurations.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckHashtable { get; set; }

        /// <summary>
        /// Whether to include hashtable key-value pairs where there is a comment
        /// between the key and the equals sign in alignment.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool AlignHashtableKvpWithInterveningComment { get; set; }

        /// <summary>
        /// Check the members of an enum.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckEnums { get; set; }

        /// <summary>
        /// Include enum members without explicit values in the width calculation.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool IncludeValuelessEnumMembers { get; set; }

        /// <summary>
        /// Whether to include enum members where there is a comment
        /// between the name and the equals sign in alignment.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool AlignEnumMemberWithInterveningComment { get; set; }

        /// <summary>
        /// A mapping of line numbers to the indices of assignment operator
        /// tokens on those lines.
        /// </summary>
        private readonly Dictionary<int, List<int>> assignmentOperatorIndicesByLine =
            new Dictionary<int, List<int>>();

        /// <summary>
        /// Analyzes the given ast to find if consecutive assignment statements are aligned.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            // The high-level approach of the rule is to find all of the
            // Key-Value pairs in a hashtable, or the members of an enum.
            // For all of these assignments, we want to locate where both the
            // left-hand-side (LHS) ends and where the equals sign is.
            // Looking at all of these assignments for a particular structure,
            // we can then decide where the equals sign _should_ be. It should
            // be in the column after the longest LHS.
            //
            // Looking at where it _is_ vs where it _should_ be, we can then
            // generate diagnostics and corrections.

            // As an optimisation, we first build a dictionary of all of the
            // assignment operators in the script, keyed by line number. We do
            // this by doing a single scan of the tokens. This makes it trvially
            // fast to find the `Equals` token for a given assignment.

            // Note: In instances where there is a parse error, we do not have
            //       access to the tokens, so we can't build this dictionary.
            //       This is relevant for the DSC configuration parsing.
            LocateAssignmentOperators();

            if (CheckHashtable)
            {
                // Find all hashtables
                var hashtableAsts = ast.FindAll(
                    a => a is HashtableAst, true
                    ).Cast<HashtableAst>();
                foreach (var hashtableAst in hashtableAsts)
                {
                    // For each hashtable find all assignment sites that meet
                    // our criteria for alignment checking
                    var hashtableAssignmentSites = ParseHashtable(hashtableAst);

                    // Check alignment of the assignment sites and emit a
                    // diagnostic for each misalignment found.
                    foreach (var diag in CheckAlignment(hashtableAssignmentSites))
                    {
                        yield return diag;
                    }
                }

                // DSC does design time checking of available resource nodes.
                // If a resource is not available at design time, the parser
                // will error. A DSC Resource definition for a resource which is
                // not found will not successfully be parsed and appear in the
                // AST as a hashtable. The below is a best-effort attempt to
                // find these assignment statements and consistently align them.

                // Find all ConfigurationDefinitionAsts
                var dscConfigDefAsts = ast.FindAll(
                    a => a is ConfigurationDefinitionAst, true
                    ).Cast<ConfigurationDefinitionAst>();
                foreach (var dscConfigDefAst in dscConfigDefAsts)
                {
                    // Within each ConfigurationDefinitionAst, there can be many
                    // nested NamedBlocks, each of which can contain many nested
                    // CommandAsts. The CommandAsts which have 3 command
                    // elements, with the middle one being an equals sign, are
                    // the ones we're interested in. `ParseDscConfigDef` will
                    // emit parsed lists of these CommandAsts that share the
                    // same parent (and so should be aligned with one another).
                    foreach (var group in ParseDscConfigDef(dscConfigDefAst, ast))
                    {
                        // Check alignment of the assignment sites and emit a
                        // diagnostic for each misalignment found.
                        foreach (var diag in CheckAlignment(group))
                        {
                            yield return diag;
                        }
                    }
                }
            }

            if (CheckEnums)
            {
                // Find all enum TypeDefinitionAsts
                var EnumTypeDefAsts = ast.FindAll(
                    a => a is TypeDefinitionAst t && t.IsEnum, true
                    ).Cast<TypeDefinitionAst>();
                foreach (var enumTypeDefAst in EnumTypeDefAsts)
                {
                    // For each enum TypeDef find all assignment sites that meet
                    // our criteria for alignment checking
                    var enumAssignmentSites = ParseEnums(enumTypeDefAst);

                    // Check alignment of the assignment sites and emit a
                    // diagnostic for each misalignment found.
                    foreach (var diag in CheckAlignment(enumAssignmentSites))
                    {
                        yield return diag;
                    }
                }
            }
        }

        /// <summary>
        /// Locate all the assignment tokens in the script and store their
        /// indices in the assignmentOperatorIndicesByLine dictionary.
        /// </summary>
        private void LocateAssignmentOperators()
        {
            // Clear any existing entries
            assignmentOperatorIndicesByLine.Clear();

            var tokens = Helper.Instance.Tokens;
            // Iterate through all tokens, looking for Equals tokens
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Kind == TokenKind.Equals)
                {
                    // When an equals token is found, check if the dictionary
                    // has an entry for this line number, and if not create one.
                    int lineNumber = tokens[i].Extent.StartLineNumber;
                    if (!assignmentOperatorIndicesByLine.ContainsKey(lineNumber))
                    {
                        assignmentOperatorIndicesByLine[lineNumber] = new List<int>();
                    }
                    // Add the index of this token to the list for this line
                    assignmentOperatorIndicesByLine[lineNumber].Add(i);
                }
            }
        }

        /// <summary>
        /// Parse a hashtable's key-value pairs into a list of tuples which are
        /// later used to verify and correct alignment of assignment operators.
        /// </summary>
        /// <param name="hashtableAst">The hashtable AST to parse.</param>
        /// <returns>
        /// A list of tuples, where each tuple is a (lhsTokenExtent, equalsExtent)
        /// pair representing the extent of the token immediately before the '='
        /// (effectively the key/rightmost key token) and the extent of the '=' itself.
        /// Only includes pairs where an '=' token is found on the same line as the key.
        /// Implicitly skips line continuations.
        /// </returns>
        private List<Tuple<IScriptExtent, IScriptExtent>> ParseHashtable(HashtableAst hashtableAst)
        {
            var assignmentSites = new List<Tuple<IScriptExtent, IScriptExtent>>();

            if (hashtableAst == null) { return assignmentSites; }

            // Enumerate the KeyValuePairs of this hashtable
            // Each KVP is a Tuple<ExpressionAst, ExpressionAst>
            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                // If the assignmentOperator dictionary has no entry for the
                // line that the key ends on, skip this KVP
                if (!assignmentOperatorIndicesByLine.ContainsKey(kvp.Item1.Extent.EndLineNumber))
                {
                    continue;
                }

                // Next we need to find the location of the equals sign for this
                // Key-Value pair. We know the line it should be on. We can
                // search all of the equals signs on that line for the one that
                // lives between the end of the key and the start of the value.

                int equalsTokenIndex = -1;
                foreach (var index in assignmentOperatorIndicesByLine[kvp.Item1.Extent.EndLineNumber])
                {
                    if (Helper.Instance.Tokens[index].Extent.StartOffset >= kvp.Item1.Extent.EndOffset &&
                        Helper.Instance.Tokens[index].Extent.EndOffset <= kvp.Item2.Extent.StartOffset
                    )
                    {
                        equalsTokenIndex = index;
                        break;
                    }
                }

                // If we didn't find the equals sign - skip this KVP
                if (equalsTokenIndex == -1)
                {
                    continue;
                }

                // Normally a Key-Value pair looks like:
                //
                //   Key = Value
                //
                // But the below is also valid:
                //
                //   Key <#Inline Comment#> = Value
                //
                // We can still use this KVP for alignment - we simply treat
                // the end of the token before the equals sign as the Left-Hand
                // Side (LHS) of the assignment. We expose a user setting for
                // this.
                // If the user has not chosen to align such KVPs and the token
                // before the equals sign does not end at the same offset as
                // the key, we skip this KVP.
                if (!AlignHashtableKvpWithInterveningComment &&
                    Helper.Instance.Tokens[equalsTokenIndex - 1].Extent.EndOffset != kvp.Item1.Extent.EndOffset
                )
                {
                    continue;
                }

                assignmentSites.Add(new Tuple<IScriptExtent, IScriptExtent>(
                    Helper.Instance.Tokens[equalsTokenIndex - 1].Extent,
                    Helper.Instance.Tokens[equalsTokenIndex].Extent
                ));
            }

            return assignmentSites;
        }

        /// <summary>
        /// Parse a DSC configuration definition's resource/property blocks into
        /// a list of tuples which are later used to verify and correct alignment of
        /// assignment operators.
        /// </summary>
        /// <param name="configDefAst">The ConfigurationDefinitionAst to parse.</param>
        /// <returns>
        /// An enumeration of lists of tuples, where each tuple is a (lhsTokenExtent, equalsExtent)
        /// pair representing the extent of the token immediately before the '='
        /// (effectively the key/rightmost key token) and the extent of the '=' itself.
        /// Only includes pairs where an '=' token is found on the same line as the key.
        /// Implicitly skips line continuations.
        /// </returns>
        private IEnumerable<List<Tuple<IScriptExtent, IScriptExtent>>> ParseDscConfigDef(
            ConfigurationDefinitionAst configDefAst,
            Ast ast
        )
        {

            if (configDefAst == null) { yield break; }

            // Find command asts shaped like: <Identifier> = <Value>
            var commandAsts = configDefAst.FindAll(
                a =>
                    a is CommandAst c &&
                    c.CommandElements.Count == 3 &&
                    c.CommandElements[1].Extent?.Text == "=",
                true
                ).Cast<CommandAst>();

            // Group by grandparent NamedBlock (commandAst.Parent is PipelineAst)
            var grouped = commandAsts.GroupBy(
                c => c.Parent?.Parent
                );

            foreach (var group in grouped)
            {
                var assignmentSites = new List<Tuple<IScriptExtent, IScriptExtent>>();

                foreach (var cmd in group)
                {
                    var lhs = cmd.CommandElements[0].Extent;
                    var eq = cmd.CommandElements[1].Extent;

                    if (lhs.EndLineNumber != eq.StartLineNumber)
                    {
                        // Skip if the key and equals sign are not on the same
                        // line
                        continue;
                    }

                    // Note: We can't use the token dictionary here like we do
                    //       for hashtables/enums, as we get here typically
                    //       because there's a parse error. i.e.
                    //       ModuleNotFoundDuringParse and ResourceNotDefined
                    //       Helper.Instance.Tokens is unavailable when there's
                    //       a parse error so we can only use the ast.

                    // In lieu of being able to check tokens, we check the
                    // source text between the end of the lhs and the start of
                    // the equals sign for non-whitespace characters.
                    //
                    // key <#comment#> = value
                    //    ^           ^
                    //    |           |
                    //    -------------
                    //         |
                    // We check for non-whitespace characters here
                    //
                    // If there are any, we extend the lhs extent to include
                    // them, so that the alignment is to the end of the
                    // rightmost non-whitespace characters.

                    // We get the text between between lhs and eq, trim it from
                    // the end (so we keep the right-most non-whitespace
                    // characters). It's length is how much we need to extend
                    // the lhs extent by.
                    var nonWhitespaceLength =
                        ast.Extent.Text.Substring(
                            lhs.EndOffset,
                            eq.StartOffset - lhs.EndOffset
                        ).TrimEnd().Length;

                    // If there's any non-whitespace characters between the
                    // key and the equals sign, and the user has chosen to
                    // ignore such cases, skip this KVP.
                    if (nonWhitespaceLength > 0 && !AlignHashtableKvpWithInterveningComment)
                    {
                        continue;
                    }

                    IScriptExtent leftExtent = null;
                    if (nonWhitespaceLength == 0)
                    {
                        // When there is no intervening comment, we use the
                        // key's extent as the LHS extent.
                        leftExtent = lhs;
                    }
                    else
                    {
                        // When there is an intervening comment, we extend
                        // the key's extent to include it.
                        leftExtent = new ScriptExtent(
                            new ScriptPosition(
                                lhs.File,
                                lhs.StartLineNumber,
                                lhs.StartColumnNumber,
                                null
                            ),
                            new ScriptPosition(
                                lhs.File,
                                lhs.EndLineNumber,
                                lhs.EndColumnNumber + nonWhitespaceLength,
                                null
                            )
                        );
                    }

                    assignmentSites.Add(new Tuple<IScriptExtent, IScriptExtent>(
                        leftExtent,
                        eq
                    ));
                }
                if (assignmentSites.Count > 0)
                {
                    yield return assignmentSites;
                }
            }
        }

        /// <summary>
        /// Parse an enum's members into a list of tuples which are later used to
        /// verify and correct alignment of assignment operators.
        /// </summary>
        /// <param name="enumTypeDefAst">The enum TypeDefinitionAst to parse.</param>
        /// <returns>
        /// A list of tuples, where each tuple is a (lhsTokenExtent, equalsExtent)
        /// pair representing the extent of the token immediately before the '='
        /// (effectively the member name) and the extent of the '=' itself.
        /// Implicitly skips line continuations.
        /// </returns>
        private List<Tuple<IScriptExtent, IScriptExtent>> ParseEnums(
            TypeDefinitionAst enumTypeDefAst
        )
        {
            var assignmentSites = new List<Tuple<IScriptExtent, IScriptExtent>>();
            if (enumTypeDefAst == null) { return assignmentSites; }

            // Ensure we're only processing enums
            if (!enumTypeDefAst.IsEnum) { return assignmentSites; }

            // Enumerate Enum Members that are PropertyMemberAst
            foreach (
                var member in enumTypeDefAst.Members.Where(
                    m => m is PropertyMemberAst
                ).Cast<PropertyMemberAst>()
            )
            {

                // Enums can have members with or without explicit values.

                // If InitialValue is null, this member has no explicit
                // value and so should have no equals sign.
                if (member.InitialValue == null)
                {
                    if (!IncludeValuelessEnumMembers)
                    {
                        continue;
                    }

                    if (member.Extent.StartLineNumber != member.Extent.EndLineNumber)
                    {
                        // This member spans multiple lines - skip it
                        continue;
                    }

                    // We include this member in the alignment check, but
                    // with a null equalsExtent. This will be ignored in
                    // CheckAlignment, but will ensure that this member
                    // is included in the calculation of the target column.
                    assignmentSites.Add(new Tuple<IScriptExtent, IScriptExtent>(
                        member.Extent,
                        null
                    ));
                    continue;
                }

                // If the assignmentOperator dictionary has no entry for the
                // line of the member name - skip this member; it should
                // have an explicit value, so must have an equals sign.
                // It's possible that the equals sign is on a different
                // line thanks to line continuations (`). We skip such
                // members.
                if (!assignmentOperatorIndicesByLine.ContainsKey(member.Extent.StartLineNumber))
                {
                    continue;
                }

                // Next we need to find the location of the equals sign for this
                // member. We know the line it should be on. We can
                // search all of the equals signs on that line.
                // 
                // Unlike hashtables, we don't have an extent for the LHS and
                // RHS of the member. We have the extent of the entire
                // member, the name of the member, and the extent of the
                // InitialValue (RHS). We can use these to find the equals
                // sign. We know the equals sign must be after the
                // member name, and before the InitialValue.

                int equalsTokenIndex = -1;
                foreach (var index in assignmentOperatorIndicesByLine[member.Extent.StartLineNumber])
                {
                    if (Helper.Instance.Tokens[index].Extent.StartOffset >= (member.Extent.StartColumnNumber + member.Name.Length) &&
                        Helper.Instance.Tokens[index].Extent.EndOffset < member.InitialValue.Extent.StartOffset
                    )
                    {
                        equalsTokenIndex = index;
                        break;
                    }
                }

                // If we didn't find the equals sign - skip, it's likely on a
                // different line due to line continuations.
                if (equalsTokenIndex == -1)
                {
                    continue;
                }

                // Normally a member with a value looks like:
                //
                //   Name = Value
                //
                // But the below is also valid:
                //
                //   Name <#Inline Comment#> = Value
                //
                // We can still use this member for alignment - we simply treat
                // the end of the token before the equals sign as the Left-Hand
                // Side (LHS) of the assignment. We expose a user setting for
                // this.
                // If the user has not chosen to align such members and the
                // token before the equals sign is a comment, we skip this
                // member.
                if (!AlignEnumMemberWithInterveningComment &&
                    Helper.Instance.Tokens[equalsTokenIndex - 1].Kind == TokenKind.Comment
                )
                {
                    continue;
                }

                assignmentSites.Add(new Tuple<IScriptExtent, IScriptExtent>(
                    Helper.Instance.Tokens[equalsTokenIndex - 1].Extent,
                    Helper.Instance.Tokens[equalsTokenIndex].Extent
                ));
            }
            return assignmentSites;
        }

        /// <summary>
        /// Check alignment of assignment operators in the provided list of
        /// (lhsTokenExtent, equalsExtent) tuples, and return diagnostics for
        /// any misalignments found.
        ///
        /// From the lhsTokenExtent, we can determine the target column for
        /// alignment (the column after the longest key). We then compare the
        /// equalsExtent's start column to the target column, and if they
        /// differ, we have a misalignment and return a diagnostic.
        /// </summary>
        /// <param name="assignmentSites">
        /// A list of tuples, where each tuple is a (lhsTokenExtent, equalsExtent)
        /// pair representing the extent of the token immediately before the '='
        /// and the extent of the '=' itself.
        /// Only includes pairs where an '=' token is found on the same line as
        /// the key.
        /// </param>
        /// <returns>
        /// An enumerable of DiagnosticRecords, one for each misaligned
        /// assignment operator found.
        /// </returns>
        private IEnumerable<DiagnosticRecord> CheckAlignment(
            List<Tuple<IScriptExtent, IScriptExtent>> assignmentSites
        )
        {
            if (assignmentSites == null || assignmentSites.Count == 0)
            {
                yield break;
            }

            // Filter out everything from assignmentSites that is not on
            // it's own line. Do this by grouping by the start line number
            // of the lhsTokenExtent, and only keeping groups with a count
            // of 1.
            assignmentSites = assignmentSites
                .GroupBy(t => t.Item1.StartLineNumber)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // If, after filtering, we have no assignment sites, exit
            if (assignmentSites == null || assignmentSites.Count == 0)
            {
                yield break;
            }

            // The target column for this hashtable is longest key plus one
            // space.
            var targetColumn = assignmentSites
                .Max(t => t.Item1.EndColumnNumber) + 1;

            // Check each element of the hashtable to see if it's aligned
            foreach (var site in assignmentSites)
            {
                // If the equalsExtent is null, this is a member without
                // an explicit value. We include such members in the
                // calculation of the target column, but we don't
                // generate diagnostics for them.
                if (site.Item2 == null)
                {
                    continue;
                }

                // If the equals sign is already at the target column,
                // no diagnostic is needed.
                if (site.Item2.StartColumnNumber == targetColumn)
                {
                    continue;
                }

                yield return new DiagnosticRecord(
                    string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementError),
                    site.Item2,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    site.Item1.File,
                    null,
                    GetCorrectionExtent(
                        site.Item1,
                        site.Item2,
                        targetColumn
                    )
                );
            }
        }

        /// <summary>
        /// Generate the correction extent to align the assignment operator
        /// to the target column.
        /// </summary>
        /// <param name="lhsExtent">The extent of the token immediately before the '='</param>
        /// <param name="equalsExtent">The extent of the '=' token</param>
        /// <param name="targetColumn">The target column to align to</param>
        /// <returns>An enumerable of CorrectionExtents, one for each correction</returns>
        private List<CorrectionExtent> GetCorrectionExtent(
            IScriptExtent lhsExtent,
            IScriptExtent equalsExtent,
            int targetColumn
        )
        {
            // We generate a correction extent which replaces the text between
            // the end of the lhs and the start of the equals sign with the
            // appropriate number of spaces to align the equals sign to the
            // target column.
            return new List<CorrectionExtent>
            {
                new CorrectionExtent(
                    lhsExtent.EndLineNumber,
                    equalsExtent.StartLineNumber,
                    lhsExtent.EndColumnNumber,
                    equalsExtent.StartColumnNumber,
                    new string(' ', targetColumn - lhsExtent.EndColumnNumber),
                    string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementError)
                )
            };
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementDescription);
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
                Strings.AlignAssignmentStatementName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
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
    }
}
