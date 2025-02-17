// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Collections.Concurrent;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// Use CmdletCorrectly: Check that cmdlets are invoked with the correct mandatory parameter
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseCmdletCorrectly : IScriptRule
    {
        // Cache of the mandatory parameters of cmdlets in PackageManagement
        // Key:   Cmdlet name
        // Value: List of mandatory parameters
        private static readonly ConcurrentDictionary<string, IReadOnlyList<string>> s_pkgMgmtMandatoryParameters =
            new ConcurrentDictionary<string, IReadOnlyList<string>>(new Dictionary<string, IReadOnlyList<string>>
            {
                { "Find-Package", Array.Empty<string>() },
                { "Find-PackageProvider", Array.Empty<string>() },
                { "Get-Package", Array.Empty<string>() },
                { "Get-PackageProvider", Array.Empty<string>() },
                { "Get-PackageSource", Array.Empty<string>() },
                { "Import-PackageProvider", new string[] { "Name" } },
                { "Install-Package", new string[] { "Name" } },
                { "Install-PackageProvider", new string[] { "Name" } },
                { "Register-PackageSource", new string[] { "ProviderName" } },
                { "Save-Package", new string[] { "Name", "InputObject" } },
                { "Set-PackageSource", new string[] { "Name", "Location" } },
                { "Uninstall-Package", new string[] { "Name", "InputObject" } },
                { "Unregister-PackageSource", new string[] { "Name", "InputObject" } },
            });

        /// <summary>
        /// AnalyzeScript: Check that cmdlets are invoked with the correct mandatory parameter
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all CommandAsts.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // Iterates all CommandAsts and check the command name.
            foreach (Ast foundAst in foundAsts)
            {
                CommandAst cmdAst = (CommandAst)foundAst;

                // Handles the exception caused by commands like, {& $PLINK $args 2> $TempErrorFile}.
                // You can also review the remark section in following document,
                // MSDN: CommandAst.GetCommandName Method
                if (cmdAst.GetCommandName() == null) continue;

                // Checks mandatory parameters.
                if (!MandatoryParameterExists(cmdAst))
                {
                    yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.UseCmdletCorrectlyError, cmdAst.GetCommandName()),
                        cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        /// <summary>
        /// Return true if mandatory parameters are used OR the cmdlet does not exist
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        private bool MandatoryParameterExists(CommandAst cmdAst)
        {
            #region Compares parameter list and mandatory parameter list.

            CommandInfo cmdInfo = Helper.Instance.GetCommandInfo(cmdAst.GetCommandName());

            // If we can't resolve the command or it's not a cmdlet, we are done
            if (cmdInfo == null || (cmdInfo.CommandType != System.Management.Automation.CommandTypes.Cmdlet))
            {
                return true;
            }

            // We can't statically analyze splatted variables, so ignore them
            if (Helper.Instance.HasSplattedVariable(cmdAst))
            {
                return true;
            }

            // Positional parameters could be mandatory, so we assume all is well
            if (Helper.Instance.PositionalParameterUsed(cmdAst) && Helper.Instance.IsKnownCmdletFunctionOrExternalScript(cmdAst, out _))
            {
                return true;
            }

            // If the command is piped to, this also precludes mandatory parameters
            if (cmdAst.Parent is PipelineAst parentPipeline
                && parentPipeline.PipelineElements.Count > 1
                && parentPipeline.PipelineElements[0] != cmdAst)
            {
                return true;
            }

            // We want to check cmdlets from PackageManagement separately because they experience a deadlock
            // when cmdInfo.Parameters or cmdInfo.ParameterSets is accessed.
            // See https://github.com/PowerShell/PSScriptAnalyzer/issues/1297
            if (s_pkgMgmtMandatoryParameters.TryGetValue(cmdInfo.Name, out IReadOnlyList<string> pkgMgmtCmdletMandatoryParams))
            {
                // If the command has no parameter sets with mandatory parameters, we are done
                if (pkgMgmtCmdletMandatoryParams.Count == 0)
                {
                    return true;
                }

                // We make the following simplifications here that all apply to the PackageManagement cmdlets:
                //   - Only one mandatory parameter per parameter set
                //   - Any part of the parameter prefix is valid
                //   - There are no parameter sets without mandatory parameters
                IEnumerable<CommandParameterAst> parameterAsts = cmdAst.CommandElements.OfType<CommandParameterAst>();
                foreach (string mandatoryParameter in pkgMgmtCmdletMandatoryParams)
                {
                    foreach (CommandParameterAst parameterAst in parameterAsts)
                    {
                        if (mandatoryParameter.StartsWith(parameterAst.ParameterName))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            // Gets mandatory parameters from cmdlet.
            // If cannot find any mandatory parameter, it's not necessary to do a further check for current cmdlet.
            var mandatoryParameters = new List<ParameterMetadata>();
            try
            {
                int noOfParamSets = cmdInfo.ParameterSets.Count;
                foreach (ParameterMetadata pm in cmdInfo.Parameters.Values)
                {
                    int count = 0;

                    if (pm.Attributes.Count < noOfParamSets)
                    {
                        continue;
                    }

                    foreach (Attribute attr in pm.Attributes)
                    {
                        if (!(attr is ParameterAttribute)) continue;
                        if (((ParameterAttribute)attr).Mandatory)
                        {
                            count += 1;
                        }
                    }

                    if (count >= noOfParamSets)
                    {
                        mandatoryParameters.Add(pm);
                    }
                }
            }
            catch (Exception)
            {
                // For cases like cmd.exe. Also for runtime exception
                return true;
            }

            if (mandatoryParameters.Count == 0)
            {
                return true;
            }

            // Compares parameter list and mandatory parameter list.
            foreach (CommandElementAst commandElementAst in cmdAst.CommandElements.OfType<CommandParameterAst>())
            {
                CommandParameterAst cpAst = (CommandParameterAst)commandElementAst;
                if (mandatoryParameters.Count<ParameterMetadata>(item =>
                    item.Name.Equals(cpAst.ParameterName, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    return true;
                }
            }

            #endregion

            return false;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseCmdletCorrectlyName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCmdletCorrectlyCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCmdletCorrectlyDescription);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




