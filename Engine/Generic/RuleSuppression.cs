using System;
using System.Linq;
using System.Management.Automation.Language;

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
        public int StartOffSet
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
                        Error = "All the arguments of the suppression message attribute should be string constant";
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
                            Error = "Named arguments must always come after positional arguments";
                            break;
                        }
                        else if (!(name.Argument is StringConstantExpressionAst))
                        {
                            Error = "All the arguments of the suppression message attribute should be string constant";
                            break;
                        }

                        switch (name.ArgumentName.ToLower())
                        {
                            case "rulename":
                                if (!String.IsNullOrWhiteSpace(RuleName))
                                {
                                    Error = "RuleName cannot be set by both positional and named arguments";
                                }

                                RuleName = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            case "rulesuppressionid":
                                if (!String.IsNullOrWhiteSpace(RuleSuppressionID))
                                {
                                    Error = "RuleSuppressionID cannot be set by both positional and named arguments";
                                }

                                RuleSuppressionID = (name.Argument as StringConstantExpressionAst).Value;
                                goto default;

                            default:
                                break;
                        }
                    }
                }
            }

            StartOffSet = start;
            EndOffset = end;
        }
    }
}
