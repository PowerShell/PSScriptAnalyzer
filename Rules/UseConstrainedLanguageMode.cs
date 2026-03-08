// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Management.Automation;

#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseConstrainedLanguageMode: Checks for patterns that indicate Constrained Language Mode should be considered.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConstrainedLanguageMode : ConfigurableRule
    {
        // Allowed COM objects in Constrained Language Mode
        private static readonly HashSet<string> AllowedComObjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Scripting.Dictionary",
            "Scripting.FileSystemObject",
            "VBScript.RegExp"
        };

        // Allowed types in Constrained Language Mode (type accelerators and common types)
        private static readonly HashSet<string> AllowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "adsi", "adsisearcher", "Alias", "AllowEmptyCollection", "AllowEmptyString",
            "AllowNull", "ArgumentCompleter", "ArgumentCompletions", "array", "bigint",
            "bool", "byte", "char", "cimclass", "cimconverter", "ciminstance", "CimSession",
            "cimtype", "CmdletBinding", "cultureinfo", "datetime", "decimal", "double",
            "DscLocalConfigurationManager", "DscProperty", "DscResource", "ExperimentAction",
            "Experimental", "ExperimentalFeature", "float", "guid", "hashtable", "int",
            "int16", "int32", "int64", "ipaddress", "IPEndpoint", "long", "mailaddress",
            "Microsoft.PowerShell.Commands.ModuleSpecification", "NoRunspaceAffinity",
            "NullString", "Object[]", "ObjectSecurity", "ordered", "OutputType", "Parameter",
            "PhysicalAddress", "pscredential", "pscustomobject", "PSDefaultValue",
            "pslistmodifier", "psobject", "psprimitivedictionary", "PSTypeNameAttribute",
            "regex", "sbyte", "securestring", "semver", "short", "single", "string",
            "SupportsWildcards", "switch", "timespan", "uint", "uint16", "uint32", "uint64",
            "ulong", "uri", "ushort", "ValidateCount", "ValidateDrive", "ValidateLength",
            "ValidateNotNull", "ValidateNotNullOrEmpty", "ValidateNotNullOrWhiteSpace",
            "ValidatePattern", "ValidateRange", "ValidateScript", "ValidateSet",
            "ValidateTrustedData", "ValidateUserDrive", "version", "void", "WildcardPattern",
            "wmi", "wmiclass", "wmisearcher", "X500DistinguishedName", "X509Certificate", "xml",
            // Full type names for common allowed types
            "System.Object", "System.String", "System.Int32", "System.Boolean", "System.Byte",
            "System.Collections.Hashtable", "System.DateTime", "System.Version", "System.Uri",
            "System.Guid", "System.TimeSpan", "System.Management.Automation.PSCredential",
            "System.Management.Automation.PSObject", "System.Security.SecureString",
            "System.Text.RegularExpressions.Regex", "System.Xml.XmlDocument",
            "System.Collections.ArrayList", "System.Collections.Generic.List",
            "System.Net.IPAddress", "System.Net.Mail.MailAddress"
        };

        public UseConstrainedLanguageMode()
        {
            // This rule is disabled by default - users must explicitly enable it
            Enable = false;
        }

        /// <summary>
        /// Checks if a type name is allowed in Constrained Language Mode
        /// </summary>
        private bool IsTypeAllowed(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return true; // Can't determine, so don't flag
            }

            // Check exact match first
            if (AllowedTypes.Contains(typeName))
            {
                return true;
            }

            // Check simple name (last part after last dot)
            if (typeName.Contains('.'))
            {
                var simpleTypeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
                if (AllowedTypes.Contains(simpleTypeName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Analyzes the script to check for patterns that may require Constrained Language Mode.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var diagnosticRecords = new List<DiagnosticRecord>();

            // Check for Add-Type usage (not allowed in Constrained Language Mode)
            var addTypeCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("Add-Type", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in addTypeCommands)
            {
                diagnosticRecords.Add(
                    new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeAddTypeError),
                        cmd.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    ));
            }

            // Check for New-Object with COM objects and TypeName (only specific ones are allowed in CLM)
            var newObjectCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("New-Object", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in newObjectCommands)
            {
                // Use StaticParameterBinder to reliably get parameter values
                var bindingResult = StaticParameterBinder.BindCommand(cmd, true);
                
                // Check for -ComObject parameter
                if (bindingResult.BoundParameters.ContainsKey("ComObject"))
                {
                    string comObjectValue = null;
                    
                    // Try to get the value from the AST directly first
                    if (bindingResult.BoundParameters["ComObject"].Value is StringConstantExpressionAst strAst)
                    {
                        comObjectValue = strAst.Value;
                    }
                    else
                    {
                        // Fall back to ConstantValue
                        comObjectValue = bindingResult.BoundParameters["ComObject"].ConstantValue as string;
                    }
                    
                    // Only flag if COM object name was found AND it's not in the allowed list
                    if (!string.IsNullOrWhiteSpace(comObjectValue) && !AllowedComObjects.Contains(comObjectValue))
                    {
                        diagnosticRecords.Add(
                            new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, 
                                    Strings.UseConstrainedLanguageModeComObjectError, 
                                    comObjectValue),
                                cmd.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                fileName
                            ));
                    }
                }
                
                // Check for -TypeName parameter
                if (bindingResult.BoundParameters.ContainsKey("TypeName"))
                {
                    var typeNameValue = bindingResult.BoundParameters["TypeName"].ConstantValue as string;
                    
                    // If ConstantValue is null, try to extract from the AST Value
                    if (typeNameValue == null && bindingResult.BoundParameters["TypeName"].Value is StringConstantExpressionAst typeStrAst)
                    {
                        typeNameValue = typeStrAst.Value;
                    }
                    
                    // Only flag if type name was found AND it's not in the allowed list
                    if (!string.IsNullOrWhiteSpace(typeNameValue) && !IsTypeAllowed(typeNameValue))
                    {
                        diagnosticRecords.Add(
                            new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, 
                                    Strings.UseConstrainedLanguageModeNewObjectError, 
                                    typeNameValue),
                                cmd.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                fileName
                            ));
                    }
                }
            }

            // Check for XAML usage (not allowed in Constrained Language Mode)
            var xamlPatterns = ast.FindAll(testAst =>
                testAst is StringConstantExpressionAst strAst &&
                strAst.Value.Contains("<") && strAst.Value.Contains("xmlns"),
                true);

            foreach (StringConstantExpressionAst xamlAst in xamlPatterns)
            {
                if (xamlAst.Value.Contains("http://schemas.microsoft.com/winfx"))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeXamlError),
                            xamlAst.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            // Check for dot-sourcing - PowerShell doesn't have a specific DotSourceExpressionAst
            // We look for patterns where a script block or file is dot-sourced
            // This is best detected through token analysis, but for simplicity we'll check for common patterns
            var scriptBlocks = ast.FindAll(testAst => testAst is ScriptBlockExpressionAst, true);
            
            foreach (ScriptBlockExpressionAst sbAst in scriptBlocks)
            {
                // Check if preceded by a dot token (basic heuristic for dot-sourcing)
                // More sophisticated detection would require token analysis
                var parent = sbAst.Parent;
                if (parent is CommandAst cmdAst)
                {
                    // Check if this looks like a dot-source pattern
                    var cmdName = cmdAst.GetCommandName();
                    if (cmdName != null && cmdName.StartsWith("."))
                    {
                        diagnosticRecords.Add(
                            new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeDotSourceError),
                                sbAst.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                fileName
                            ));
                    }
                }
            }

            // Check for Invoke-Expression usage (restricted in Constrained Language Mode)
            var invokeExpressionCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("Invoke-Expression", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in invokeExpressionCommands)
            {
                diagnosticRecords.Add(
                    new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeInvokeExpressionError),
                        cmd.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    ));
            }

            // Check for disallowed type accelerators and type constraints
            var typeConstraints = ast.FindAll(testAst => testAst is TypeConstraintAst, true);
            foreach (TypeConstraintAst typeConstraint in typeConstraints)
            {
                var typeName = typeConstraint.TypeName.FullName;
                if (!IsTypeAllowed(typeName))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture,
                                Strings.UseConstrainedLanguageModeConstrainedTypeError,
                                typeName),
                            typeConstraint.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeDescription);
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
                Strings.UseConstrainedLanguageModeName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Information;
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
