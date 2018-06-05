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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    // Rule name is inconsistent.
    /// <summary>
    /// AvoidInvokingEmptyMembers: Analyzes the script to check if any non-constant members have been invoked.
    /// </summary>    
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidInvokingEmptyMembers : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the script to check if any non-constant members have been invoked.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> memberExpression = ast.FindAll(testAst => testAst is MemberExpressionAst, true);
            foreach (MemberExpressionAst member in memberExpression)
            {
                string context = member.Member.Extent.ToString();
                if (context.Contains("("))
                {
                    //check if parenthesis and have non-constant members
                    IEnumerable<Ast> binaryExpression = member.FindAll(
                        binaryAst => binaryAst is BinaryExpressionAst, true);
                    if (binaryExpression.Any())
                    {
                        foreach (BinaryExpressionAst bin in binaryExpression)
                        {
                            if (!bin.Operator.Equals(null))
                            {
                                yield return
                                    new DiagnosticRecord(
                                        string.Format(CultureInfo.CurrentCulture,
                                            Strings.AvoidInvokingEmptyMembersError,
                                            context),
                                        member.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
                    }
                }
            }
        
        }

        
        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidInvokingEmptyMembersName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidInvokingEmptyMembersCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidInvokingEmptyMembersDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
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
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




