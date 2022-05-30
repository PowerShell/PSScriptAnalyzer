// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingBrokenHashAlgorithms: Avoid using the broken algorithms MD5 or SHA-1.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingBrokenHashAlgorithms : AvoidParameterGeneric
    {
        private readonly string[] brokenAlgorithms = new string[]
        {
            "MD5",
            "SHA1",
        };

        private string algorithm;

        /// <summary>
        /// Condition on the cmdlet that must be satisfied for the error to be raised
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override bool CommandCondition(CommandAst CmdAst)
        {
            return true;
        }

        /// <summary>
        /// Condition on the parameter that must be satisfied for the error to be raised.
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <param name="CeAst"></param>
        /// <returns></returns>
        public override bool ParameterCondition(CommandAst CmdAst, CommandElementAst CeAst)
        {
            if (CeAst is CommandParameterAst)
            {
                CommandParameterAst cmdParamAst = CeAst as CommandParameterAst;

                if (String.Equals(cmdParamAst.ParameterName, "algorithm", StringComparison.OrdinalIgnoreCase))
                {
                    Ast hashAlgorithmArgument = cmdParamAst.Argument;
                    if (hashAlgorithmArgument is null)
                    {
                        hashAlgorithmArgument = GetHashAlgorithmArg(CmdAst, cmdParamAst.Extent.StartOffset);
                        if (hashAlgorithmArgument is null)
                        {
                            return false;
                        }
                    }

                    var constExprAst = hashAlgorithmArgument as ConstantExpressionAst;
                    if (constExprAst != null)
                    {
                        algorithm = constExprAst.Value as string;
                        return IsBrokenAlgorithm(algorithm);
                    }
                }
            }

            return false;
        }

        private bool IsBrokenAlgorithm(string algorithm)
        {
            if (algorithm != null)
            {
                return brokenAlgorithms.Contains<string>(
                    algorithm,
                    StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        private Ast GetHashAlgorithmArg(CommandAst CmdAst, int StartIndex)
        {
            int small = int.MaxValue;
            Ast hashAlgorithmArg = null;
            foreach (Ast ast in CmdAst.CommandElements)
            {
                if (ast.Extent.StartOffset > StartIndex && ast.Extent.StartOffset < small)
                {
                    hashAlgorithmArg = ast;
                    small = ast.Extent.StartOffset;
                }
            }

            return hashAlgorithmArg;
        }

        /// <summary>
        /// Retrieves the error message
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override string GetError(string FileName, CommandAst CmdAst)
        {
            if (CmdAst is null)
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingBrokenHashAlgorithmsError, CmdAst.GetCommandName(), algorithm);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingBrokenHashAlgorithmsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingBrokenHashAlgorithmsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingBrokenHashAlgorithmsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// DiagnosticSeverity: Retrieves the severity of the rule of type DiagnosticSeverity: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public override DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




