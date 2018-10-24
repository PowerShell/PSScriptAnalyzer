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
using System.IO;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// DscTestsPresent: Checks that DSC tests for given resource are present.
    /// Rule expects directory Tests to be present:
    ///     For non-class based resources it should exist at the same folder level as DSCResources folder.
    ///     For class based resources it should be present at the same folder level as resource psm1 file. 
    /// Tests folder should contain test script for given resource - file name should contain resource's name.
    /// </summary>
#if !CORECLR
[Export(typeof(IDSCResourceRule))]
#endif
    public class DscTestsPresent : IDSCResourceRule
    {
        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName)
        {
            // we are given a script definition, do not analyze
            if (String.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            String fileNameOnly = Path.GetFileName(fileName);
            String resourceName = Path.GetFileNameWithoutExtension(fileNameOnly);
            String testsQuery = String.Format("*{0}*", resourceName);
            Boolean testsPresent = false;
            String expectedTestsPath = Path.Combine(new String[] { fileName, "..", "..", "..", "Tests" });

            // Verify tests are present
            if (Directory.Exists(expectedTestsPath))
            {
                DirectoryInfo testsFolder = new DirectoryInfo(expectedTestsPath);
                FileInfo[] testFiles = testsFolder.GetFiles(testsQuery, SearchOption.AllDirectories);
                if (testFiles.Length != 0)
                {
                    testsPresent = true;
                }
            }

            // Return error if no tests present
            if (!testsPresent)
            {
                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.DscTestsPresentNoTestsError, resourceName),
                            ast.Extent, GetName(), DiagnosticSeverity.Information, fileName);
            }
        }

        #if !(PSV3||PSV4)

        /// <summary>
        /// AnalyzeDSCClass: Analyzes given DSC class
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName)
        {
            // we are given a script definition, do not analyze
            if (String.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            String resourceName = null;

            IEnumerable<Ast> dscClasses = ast.FindAll(item =>
                item is TypeDefinitionAst
                && ((item as TypeDefinitionAst).IsClass)
                && (item as TypeDefinitionAst).Attributes.Any(attr => String.Equals("DSCResource", attr.TypeName.FullName, StringComparison.OrdinalIgnoreCase)), true);

            foreach (TypeDefinitionAst dscClass in dscClasses)
            {
                resourceName = dscClass.Name;

                String testsQuery = String.Format("*{0}*", resourceName);
                Boolean testsPresent = false;
                String expectedTestsPath = Path.Combine(new String[] { fileName, "..", "Tests" });

                // Verify tests are present
                if (Directory.Exists(expectedTestsPath))
                {
                    DirectoryInfo testsFolder = new DirectoryInfo(expectedTestsPath);
                    FileInfo[] testFiles = testsFolder.GetFiles(testsQuery);
                    if (testFiles.Length != 0)
                    {
                        testsPresent = true;
                    }
                }

                // Return error if no tests present
                if (!testsPresent)
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.DscTestsPresentNoTestsError, resourceName),
                                dscClass.Extent, GetName(), DiagnosticSeverity.Information, fileName);
                }
            }
        }

        #endif

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.DscTestsPresent);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DscTestsPresentCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DscTestsPresentDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DSCSourceName);
        }
    }

}



