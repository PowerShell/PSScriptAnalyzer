// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    ///
    /// </summary>
    public class RuleSuppression
    {
        private string _ruleName;

        /// <summary>
        /// The start offset of the rule suppression attribute (not where it starts to apply)
        /// </summary>
        public int StartAttributeLine
        {
            get;
            set;
        }

        /// <summary>
        /// The start offset of the rule suppression
        /// </summary>
        public int StartOffset
        {
            get;
            set;
        }

        /// <summary>
        /// The end offset of the rule suppression
        /// </summary>
        public int EndOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the rule being suppressed
        /// </summary>
        public string RuleName
        {
            get
            {
                return _ruleName;
            }

            set
            {
                _ruleName = value;

                if (!String.IsNullOrWhiteSpace(_ruleName)
                    && (ScriptAnalyzer.Instance.ScriptRules != null
                        && ScriptAnalyzer.Instance.ScriptRules.Count(item => String.Equals(item.GetName(), _ruleName, StringComparison.OrdinalIgnoreCase)) == 0)
                    && (ScriptAnalyzer.Instance.TokenRules != null
                        && ScriptAnalyzer.Instance.TokenRules.Count(item => String.Equals(item.GetName(), _ruleName, StringComparison.OrdinalIgnoreCase)) == 0)
                    && (ScriptAnalyzer.Instance.ExternalRules != null
                        && ScriptAnalyzer.Instance.ExternalRules.Count(item => String.Equals(item.GetName(), _ruleName, StringComparison.OrdinalIgnoreCase)) == 0)
                    && (ScriptAnalyzer.Instance.DSCResourceRules != null
                        && ScriptAnalyzer.Instance.DSCResourceRules.Count(item => String.Equals(item.GetName(), _ruleName, StringComparison.OrdinalIgnoreCase)) == 0))
                {
                    Error = String.Format(Strings.RuleSuppressionRuleNameNotFound, _ruleName);
                }
            }
        }

        /// <summary>
        /// ID of the violation instance
        /// </summary>
        public string RuleSuppressionID
        {
            get;
            set;
        }

        /// <summary>
        /// Scope of the rule suppression
        /// </summary>
        public string Scope
        {
            get;
            set;
        }

        /// <summary>
        /// Target of the rule suppression
        /// </summary>
        public string Target
        {
            get;
            set;
        }

        /// <summary>
        /// Returns error occurred in trying to parse the attribute
        /// </summary>
        public string Error
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the justification for the suppression
        /// </summary>
        public string Justification
        {
            get;
            set;
        }

        private static HashSet<string> scopeSet;

        /// <summary>
        /// Initialize the scopeSet
        /// </summary>
        static RuleSuppression()
        {
            scopeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            scopeSet.Add("function");
            scopeSet.Add("class");
        }

        /// <summary>
        /// Returns rule suppression from an attribute ast that has the type suppressmessageattribute
        /// </summary>
        /// <param name="attrAst"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RuleSuppression(AttributeAst attrAst, int start, int end)
        {
            Error = String.Empty;

            if (attrAst != null)
            {
                StartAttributeLine = attrAst.Extent.StartLineNumber;
                var positionalArguments = attrAst.PositionalArguments;
                var namedArguments = attrAst.NamedArguments;

                int lastPositionalArgumentsOffset = -1;

                if (positionalArguments != null && positionalArguments.Count != 0)
                {
                    int count = positionalArguments.Count;
                    lastPositionalArgumentsOffset = positionalArguments[positionalArguments.Count - 1].Extent.StartOffset;

                    if (positionalArguments.Any(item => !(item is StringConstantExpressionAst)))
                    {
                        Error = Strings.StringConstantArgumentsSuppressionAttributeError;
                    }
                    else
                    {
                        switch (count)
                        {
                            case 5:
                                Justification = (positionalArguments[4] as StringConstantExpressionAst).Value;
                                goto case 4;

                            case 4:
                                Target = (positionalArguments[3] as StringConstantExpressionAst).Value;
                                goto case 3;

                            case 3:
                                Scope = (positionalArguments[2] as StringConstantExpressionAst).Value;
                                goto case 2;

                            case 2:
                                RuleSuppressionID = (positionalArguments[1] as StringConstantExpressionAst).Value;
                                goto case 1;

                            case 1:
                                RuleName = (positionalArguments[0] as StringConstantExpressionAst).Value;
                                goto default;

                            default:
                                break;
                        }
                    }
                }

                if (namedArguments != null && namedArguments.Count != 0)
                {
                    foreach (var name in namedArguments)
                    {
                        if (name.Extent.StartOffset < lastPositionalArgumentsOffset)
                        {
                            Error = Strings.NamedArgumentsBeforePositionalError;
                            break;
                        }
                        else if (!(name.Argument is StringConstantExpressionAst))
                        {
                            Error = Strings.StringConstantArgumentsSuppressionAttributeError;
                            break;
                        }

                        switch (name.ArgumentName.ToLower())
                        {
                            case "rulename":
                                if (!String.IsNullOrWhiteSpace(RuleName))
                                {
                                    Error = String.Format(Strings.NamedAndPositionalArgumentsConflictError, name);
                                }

                                RuleName = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            case "rulesuppressionid":
                                if (!String.IsNullOrWhiteSpace(RuleSuppressionID))
                                {
                                    Error = String.Format(Strings.NamedAndPositionalArgumentsConflictError, name);
                                }

                                RuleSuppressionID = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            case "scope":
                                if (!String.IsNullOrWhiteSpace(Scope))
                                {
                                    Error = String.Format(Strings.NamedAndPositionalArgumentsConflictError, name);
                                }

                                Scope = (name.Argument as StringConstantExpressionAst).Value;

                                if (!scopeSet.Contains(Scope))
                                {
                                    Error = Strings.WrongScopeArgumentSuppressionAttributeError;
                                }

                                goto default;

                            case "target":
                                if (!String.IsNullOrWhiteSpace(Target))
                                {
                                    Error = String.Format(Strings.NamedAndPositionalArgumentsConflictError, name);
                                }

                                Target = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            case "justification":
                                if (!String.IsNullOrWhiteSpace(Justification))
                                {
                                    Error = String.Format(Strings.NamedAndPositionalArgumentsConflictError, name);
                                }

                                Justification = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            default:
                                break;
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(Error))
                {
                    // May be cases where the rulename is null because we didn't look at the rulename after
                    // we found out there is an error
                    RuleName = String.Empty;
                }
                else if (String.IsNullOrWhiteSpace(RuleName))
                {
                    RuleName = String.Empty;
                    Error = Strings.NullRuleNameError;
                }

                // Must have scope and target together
                if (String.IsNullOrWhiteSpace(Scope) && !String.IsNullOrWhiteSpace(Target))
                {
                    Error = Strings.TargetWithoutScopeSuppressionAttributeError;
                }
            }

            StartOffset = start;
            EndOffset = end;

            if (!String.IsNullOrWhiteSpace(Error))
            {
                if (String.IsNullOrWhiteSpace(attrAst.Extent.File))
                {
                    Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormatScriptDefinition, StartAttributeLine, Error);
                }
                else
                {
                    Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormat, StartAttributeLine,
                        System.IO.Path.GetFileName(attrAst.Extent.File), Error);
                }
            }
        }

        /// <summary>
        /// Constructs rule expression from rule name, id, start, end, startAttributeLine and justification
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="ruleSuppressionID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="startAttributeLine"></param>
        /// <param name="justification"></param>
        public RuleSuppression(string ruleName, string ruleSuppressionID, int start, int end, int startAttributeLine, string justification)
        {
            RuleName = ruleName;
            RuleSuppressionID = ruleSuppressionID;
            StartOffset = start;
            EndOffset = end;
            StartAttributeLine = startAttributeLine;
            Justification = justification;
        }

        /// <summary>
        /// Given a list of attribute asts, return a list of rule suppression
        /// with startoffset at start and endoffset at end
        /// </summary>
        /// <param name="attrAsts"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<RuleSuppression> GetSuppressions(IEnumerable<AttributeAst> attrAsts, int start, int end, Ast scopeAst)
        {
            List<RuleSuppression> result = new List<RuleSuppression>();

            if (attrAsts == null || scopeAst == null)
            {
                return result;
            }

            IEnumerable<AttributeAst> suppressionAttribute = attrAsts.Where(
                item => item.TypeName.GetReflectionType() == typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute));

            foreach (var attributeAst in suppressionAttribute)
            {
                RuleSuppression ruleSupp = new RuleSuppression(attributeAst, start, end);

                // If there is no error and scope is not null
                if (String.IsNullOrWhiteSpace(ruleSupp.Error) && !String.IsNullOrWhiteSpace(ruleSupp.Scope))
                {
                    if (String.IsNullOrWhiteSpace(ruleSupp.Target))
                    {
                        ruleSupp.Target = "*";
                    }

                    // According to documentation 'target' supports regular expression. But to maintain compatibility with
                    // previous implementation we interpret '*' as a glob and therefore replace '*' with '.*'
                    // regex for wild card *
                    Regex reg = new Regex(String.Format("^{0}$", ruleSupp.Target.Replace(@"*", ".*")), RegexOptions.IgnoreCase);
                    IEnumerable<Ast> targetAsts = null;

                    switch (ruleSupp.Scope.ToLower())
                    {
                        case "function":
                            targetAsts = scopeAst.FindAll(item => item is FunctionDefinitionAst && reg.IsMatch((item as FunctionDefinitionAst).Name), true);
                            goto default;

                        #if !PSV3

                        case "class":
                            targetAsts = scopeAst.FindAll(item => item is TypeDefinitionAst && reg.IsMatch((item as TypeDefinitionAst).Name), true);
                            goto default;

                        #endif

                        default:
                            break;
                    }

                    if (targetAsts != null)
                    {
                        if (targetAsts.Count() == 0)
                        {
                            if (String.IsNullOrWhiteSpace(scopeAst.Extent.File))
                            {
                                ruleSupp.Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormatScriptDefinition, ruleSupp.StartAttributeLine,
                                    String.Format(Strings.TargetCannotBeFoundError, ruleSupp.Target, ruleSupp.Scope));
                            }
                            else
                            {
                                ruleSupp.Error = String.Format(CultureInfo.CurrentCulture, Strings.RuleSuppressionErrorFormat, ruleSupp.StartAttributeLine,
                                    System.IO.Path.GetFileName(scopeAst.Extent.File), String.Format(Strings.TargetCannotBeFoundError, ruleSupp.Target, ruleSupp.Scope));
                            }

                            result.Add(ruleSupp);
                            continue;
                        }

                        foreach (Ast targetAst in targetAsts)
                        {
                            result.Add(new RuleSuppression(ruleSupp.RuleName, ruleSupp.RuleSuppressionID, targetAst.Extent.StartOffset,
                                targetAst.Extent.EndOffset, attributeAst.Extent.StartLineNumber, ruleSupp.Justification));
                        }
                    }

                }
                else
                {
                    // this may add rule suppression that contains error but we will check for this in the engine to throw out error
                    result.Add(ruleSupp);
                }
            }

            return result;
        }
    }
}
