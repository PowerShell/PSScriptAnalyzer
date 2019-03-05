// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingDeprecatedManifestFields: Run Test Module Manifest to check that no deprecated fields are being used.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingDeprecatedManifestFields : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Run Test Module Manifest to check that no deprecated fields are being used.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }
            if (fileName == null)
            {
                yield break;
            }
            if (Helper.IsModuleManifest(fileName))
            {
                using (var ps = System.Management.Automation.PowerShell.Create())
                {
                    IEnumerable<PSObject> result = null;

                    // hash table in psd1
                    var hashTableAst = ast.FindAll(item => item is HashtableAst, false).FirstOrDefault();

                    // no hash table means not a module manifest
                    if (hashTableAst == null)
                    {
                        yield break;
                    }

                    var table = hashTableAst as HashtableAst;

                    // needs to find the PowerShellVersion key
                    foreach (var kvp in table.KeyValuePairs)
                    {
                        if (kvp.Item1 != null && kvp.Item1 is StringConstantExpressionAst)
                        {
                            var key = (kvp.Item1 as StringConstantExpressionAst).Value;

                            // find the powershellversion key in the hashtable
                            if (string.Equals(key, "PowerShellVersion", StringComparison.OrdinalIgnoreCase) && kvp.Item2 != null)
                            {
                                // get the string value of the version
                                var value = kvp.Item2.Find(item => item is StringConstantExpressionAst, false);

                                if (value != null)
                                {
                                    Version psVersion = null;

                                    // get the version
                                    if (Version.TryParse((value as StringConstantExpressionAst).Value, out psVersion))
                                    {
                                        // if version exists and version less than 3, don't raise rule
                                        if (psVersion.Major < 3)
                                        {
                                            yield break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        ps.AddCommand("Test-ModuleManifest");
                        ps.AddParameter("Path", fileName);

                        // Suppress warnings emitted during the execution of Test-ModuleManifest
                        // ModuleManifest rule must catch any violations (warnings/errors) and generate DiagnosticRecord(s)
                        ps.AddParameter("WarningAction", ActionPreference.SilentlyContinue);
                        ps.AddParameter("WarningVariable", "Message");
                        ps.AddScript("$Message");
                        result = ps.Invoke();
                    }
                    catch
                    {}

                    if (result != null)
                    {
                        foreach (var warning in result)
                        {
                            if (warning.BaseObject != null)
                            {
                                yield return
                                    new DiagnosticRecord(
                                        String.Format(CultureInfo.CurrentCulture, warning.BaseObject.ToString()), ast.Extent,
                                        GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingDeprecatedManifestFieldsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDeprecatedManifestFieldsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDeprecatedManifestFieldsDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
