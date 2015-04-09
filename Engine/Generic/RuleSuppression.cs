using System;
using System.Linq;
using System.Management.Automation.Language;
using System.Collections.Generic;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// 
    /// </summary>
    public class RuleSuppression
    {
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
            get;
            set;
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
        /// Returns error occurred in trying to parse the attribute
        /// </summary>
        public string Error
        {
            get;
            set;
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

                            default:
                                break;
                        }
                    }
                }
            }

            StartOffset = start;
            EndOffset = end;
        }

        /// <summary>
        /// Given a list of attribute asts, return a list of rule suppression
        /// with startoffset at start and endoffset at end
        /// </summary>
        /// <param name="attrAsts"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<RuleSuppression> GetSuppressions(IEnumerable<AttributeAst> attrAsts, int start, int end)
        {
            List<RuleSuppression> result = new List<RuleSuppression>();

            if (attrAsts == null)
            {
                return result;
            }

            IEnumerable<AttributeAst> suppressionAttribute = attrAsts.Where(
                item => item.TypeName.GetReflectionType() == typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute));

            foreach (var attributeAst in suppressionAttribute)
            {
                RuleSuppression ruleSupp = new RuleSuppression(attributeAst, start, end);

                if (string.IsNullOrWhiteSpace(ruleSupp.Error))
                {
                    result.Add(ruleSupp);
                }
            }

            return result;
        }
    }
}
