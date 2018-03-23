// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// This class extends AstVisitor2 and will skip any namedblockast and commandast
    /// that has certain names.
    /// </summary>
    public class SkipNamedBlock : AstVisitor
    {
        private List<string> NamesToBeSkipped = new List<string>();

        /// <summary>
        /// File name
        /// </summary>
        public string fileName;

        /// <summary>
        /// My Diagnostic Records
        /// </summary>
        public List<DiagnosticRecord> DiagnosticRecords = new List<DiagnosticRecord>();

        /// <summary>
        /// Add the names to the namestobeskipped list
        /// </summary>
        /// <param name="names"></param>
        public void AddNames(List<string> names)
        {
            NamesToBeSkipped.AddRange(names);
        }

        /// <summary>
        /// Clear NamesToBeSkipped and DiagnosticRecords list
        /// </summary>
        public void ClearList()
        {
            NamesToBeSkipped.Clear();
            DiagnosticRecords.Clear();
        }

        /// <summary>
        /// Visit NamedBlockAst. Skip if name of the block
        /// is in namestobeskipped
        /// </summary>
        /// <param name="namedBlockAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return VisitActionHelper(namedBlockAst);
        }

        /// <summary>
        /// Similar to visitnamedblock
        /// </summary>
        /// <param name="sbAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst sbAst)
        {
            return VisitActionHelper(sbAst);
        }

        private AstVisitAction VisitActionHelper(Ast ast)
        {
            if (ast == null) return AstVisitAction.SkipChildren;

            bool skipped = false;
            foreach (string name in NamesToBeSkipped)
            {
                if (Helper.Instance.SkipBlock(name, ast))
                {
                    skipped = true;
                    break;
                }
            }

            if (skipped)
            {
                return AstVisitAction.SkipChildren;
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Visit CommandAst. Skip if name of command is in
        /// namestobeskipped
        /// </summary>
        /// <param name="commandAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (commandAst == null || commandAst.GetCommandName() == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (commandAst.CommandElements != null && commandAst.CommandElements.Count > 0)
            {
                var firstCommand = commandAst.CommandElements[0];
                if (NamesToBeSkipped.Contains(firstCommand.Extent.Text, StringComparer.OrdinalIgnoreCase))
                {
                    return AstVisitAction.SkipChildren;
                }
            }

            return AstVisitAction.Continue;
        }
    }
}
