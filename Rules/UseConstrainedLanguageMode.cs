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
            "NullString", "Object", "ObjectSecurity", "ordered", "OutputType", "Parameter",
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

        /// <summary>
        /// Cache for typed variable assignments per scope to avoid O(N*M) performance issues.
        /// Key: Scope AST (FunctionDefinitionAst or ScriptBlockAst)
        /// Value: Dictionary mapping variable names to their type names
        /// </summary>
        private Dictionary<Ast, Dictionary<string, string>> _typedVariableCache;

        /// <summary>
        /// When True, ignores the presence of script signature blocks and runs all CLM checks
        /// regardless of whether a script appears to be signed.
        /// When False (default), scripts that contain a PowerShell signature block (for example,
        /// one starting with '# SIG # Begin signature block') are treated as having elevated
        /// permissions for this rule and only critical checks (dot-sourcing, parameter types,
        /// manifests) are performed. No cryptographic validation or trust evaluation of the
        /// signature is performed.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool IgnoreSignatures { get; set; }

        public UseConstrainedLanguageMode()
        {
            // This rule is disabled by default - users must explicitly enable it
            Enable = false;

            // IgnoreSignatures defaults to false (respects signatures)
            IgnoreSignatures = false;
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

            // Handle array types (e.g., string[], System.String[], int[][])
            // Strip array brackets and check the base type
            string baseTypeName = typeName;


            // Handle multi-dimensional or jagged arrays by removing all brackets
            while (baseTypeName.EndsWith("[]", StringComparison.Ordinal))
            {
                baseTypeName = baseTypeName.Substring(0, baseTypeName.Length - 2);
            }


            // Check exact match first
            if (AllowedTypes.Contains(baseTypeName))
            {
                return true;
            }

            // Check simple name (last part after last dot)
            if (baseTypeName.Contains('.'))
            {
                var simpleTypeName = baseTypeName.Substring(baseTypeName.LastIndexOf('.') + 1);
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

            // Initialize cache for this analysis to avoid O(N*M) performance issues
            _typedVariableCache = new Dictionary<Ast, Dictionary<string, string>>();

            var diagnosticRecords = new List<DiagnosticRecord>();

            // Check if the file is signed (via signature block detection)
            bool isFileSigned = IgnoreSignatures ? false : IsScriptSigned(fileName);

            // Note: If IgnoreSignatures is true, isFileSigned will always be false,
            // causing all CLM checks to run regardless of actual signature status

            // Check if this is a module manifest (.psd1 file)
            bool isModuleManifest = fileName != null && fileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase);

            if (isModuleManifest)
            {
                // Perform PSD1-specific checks
                // These checks are ALWAYS enforced, even for signed scripts
                CheckModuleManifest(ast, fileName, diagnosticRecords);
            }

            // For signed scripts, only check specific patterns that are still restricted
            // (unless IgnoreSignatures is true, then this block is skipped)
            if (isFileSigned)
            {
                // Even signed scripts have these restrictions in CLM:

                // 1. Check for dot-sourcing (still restricted in CLM even for signed scripts)
                CheckDotSourcing(ast, fileName, diagnosticRecords);

                // 2. Check for type constraints on parameters (still need to be validated)
                CheckParameterTypeConstraints(ast, fileName, diagnosticRecords);

                return diagnosticRecords;
            }

            // For unsigned scripts (or when IgnoreSignatures is true), perform all CLM checks
            CheckAllClmRestrictions(ast, fileName, diagnosticRecords);

            return diagnosticRecords;
        }

        /// <summary>
        /// Checks if a PowerShell script file appears to be digitally signed.
        /// Note: This performs a simple text check for the signature block marker.
        /// It does NOT validate signature authenticity, certificate trust, or file integrity.
        /// For production use, PowerShell's execution policy and Get-AuthenticodeSignature
        /// should be used to properly validate signatures.
        /// </summary>
        private bool IsScriptSigned(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(fileName))
            {
                return false;
            }

            // Only check .ps1, .psm1, and .psd1 files
            string extension = System.IO.Path.GetExtension(fileName);
            if (!extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                // Read the file content
                string content = System.IO.File.ReadAllText(fileName);

                // Check for signature block marker
                // A signed PowerShell script contains a signature block that starts with:
                // # SIG # Begin signature block
                //
                // IMPORTANT: This is a simple text check only. It does NOT validate:
                // - Signature authenticity
                // - Certificate validity or trust
                // - File integrity (hash matching)
                // - Certificate expiration
                //
                // This check assumes that if a signature block is present, the script
                // was intended to be signed. Actual signature validation is performed
                // by PowerShell at execution time based on execution policy.
                return content.IndexOf("# SIG # Begin signature block", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                // If we can't read the file, assume it's not signed
                return false;
            }
        }

        /// <summary>
        /// Performs all CLM restriction checks (for unsigned scripts).
        /// </summary>
        private void CheckAllClmRestrictions(Ast ast, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
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

            // Check for dot-sourcing (also called separately for signed scripts)
            CheckDotSourcing(ast, fileName, diagnosticRecords);

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

            // Check for class definitions (not allowed in Constrained Language Mode)
            var classDefinitions = ast.FindAll(testAst =>
                testAst is TypeDefinitionAst typeAst && typeAst.IsClass,
                true);

            foreach (TypeDefinitionAst classDef in classDefinitions)
            {
                diagnosticRecords.Add(
                    new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture,
                            Strings.UseConstrainedLanguageModeClassError,
                            classDef.Name),
                        classDef.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    ));
            }

            // Check for parameter type constraints (also called separately for signed scripts)
            CheckParameterTypeConstraints(ast, fileName, diagnosticRecords);

            // Check for disallowed type constraints on variables (e.g., [System.Net.WebClient]$client)
            var typeConstraints = ast.FindAll(testAst =>
                testAst is TypeConstraintAst typeConstraint &&
                !(typeConstraint.Parent is ParameterAst), // Exclude parameters - handled above
                true);

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

            // Check for disallowed type expressions and casts (e.g., [System.Net.WebClient]::new() or $x -as [Type])
            var typeExpressions = ast.FindAll(testAst => testAst is TypeExpressionAst, true);
            foreach (TypeExpressionAst typeExpr in typeExpressions)
            {
                var typeName = typeExpr.TypeName.FullName;
                if (!IsTypeAllowed(typeName))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture,
                                Strings.UseConstrainedLanguageModeTypeExpressionError,
                                typeName),
                            typeExpr.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            // Check for convert expressions (e.g., $x = [System.Net.WebClient]$value)
            var convertExpressions = ast.FindAll(testAst => testAst is ConvertExpressionAst, true);
            foreach (ConvertExpressionAst convertExpr in convertExpressions)
            {
                var typeName = convertExpr.Type.TypeName.FullName;

                // Special case: [PSCustomObject]@{} is not allowed in CLM
                // Even though PSCustomObject is an allowed type for parameters,
                // the type cast syntax with hashtable literal is blocked in CLM
                if (typeName.Equals("PSCustomObject", StringComparison.OrdinalIgnoreCase) &&
                    convertExpr.Child is HashtableAst)
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture,
                                Strings.UseConstrainedLanguageModePSCustomObjectError),
                            convertExpr.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                    continue; // Already flagged, skip general type check
                }

                if (!IsTypeAllowed(typeName))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture,
                                Strings.UseConstrainedLanguageModeConvertExpressionError,
                                typeName),
                            convertExpr.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            // Check for member invocations on disallowed types
            // This includes method calls and property access on variables with type constraints
            var memberInvocations = ast.FindAll(testAst =>
                testAst is InvokeMemberExpressionAst || testAst is MemberExpressionAst, true);

            foreach (Ast memberAst in memberInvocations)
            {
                // Skip static member access - already handled by TypeExpressionAst check
                if (memberAst is InvokeMemberExpressionAst invokeAst && invokeAst.Static)
                {
                    continue;
                }

                if (memberAst is MemberExpressionAst memAst && memAst.Static)
                {
                    continue;
                }

                // Get the expression being invoked on (e.g., the variable in $var.Method())
                ExpressionAst targetExpr = memberAst is InvokeMemberExpressionAst invExpr
                    ? invExpr.Expression
                    : ((MemberExpressionAst)memberAst).Expression;

                // Check if the target has a type constraint
                string constrainedType = GetTypeConstraintFromExpression(targetExpr);
                if (!string.IsNullOrWhiteSpace(constrainedType) && !IsTypeAllowed(constrainedType))
                {
                    string memberName = memberAst is InvokeMemberExpressionAst inv
                        ? (inv.Member as StringConstantExpressionAst)?.Value ?? "<unknown>"
                        : ((memberAst as MemberExpressionAst).Member as StringConstantExpressionAst)?.Value ?? "<unknown>";

                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture,
                                Strings.UseConstrainedLanguageModeMemberAccessError,
                                constrainedType,
                                memberName),
                            memberAst.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }
        }

        /// <summary>
        /// Checks for dot-sourcing patterns which are restricted in CLM even for signed scripts.
        /// </summary>
        private void CheckDotSourcing(Ast ast, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            // Dot-sourcing is detected by looking for commands where the extent text starts with a dot
            // Example: . $PSScriptRoot\Helper.ps1
            // Example: . .\script.ps1
            // PowerShell doesn't have a specific DotSourceExpressionAst, so we check the command extent
            var commands = ast.FindAll(testAst => testAst is CommandAst, true);

            foreach (CommandAst cmdAst in commands)
            {
                // Check if the command extent starts with a dot followed by whitespace
                // This indicates dot-sourcing
                string extentText = cmdAst.Extent.Text.TrimStart();
                if (extentText.StartsWith(".") && extentText.Length > 1 && char.IsWhiteSpace(extentText[1]))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeDotSourceError),
                            cmdAst.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }
        }

        /// <summary>
        /// Checks parameter type constraints which need validation even for signed scripts.
        /// </summary>
        private void CheckParameterTypeConstraints(Ast ast, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            // Find all parameter definitions
            var parameters = ast.FindAll(testAst => testAst is ParameterAst, true);

            foreach (ParameterAst param in parameters)
            {
                // Check for type constraints on parameters
                var typeConstraints = param.Attributes.OfType<TypeConstraintAst>();

                foreach (var typeConstraint in typeConstraints)
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
            }
        }

        /// <summary>
        /// Attempts to determine if an expression has a type constraint.
        /// Returns the type name if found, otherwise null.
        /// </summary>
        private string GetTypeConstraintFromExpression(ExpressionAst expr)
        {
            if (expr == null)
            {
                return null;
            }

            // Check if this is a convert expression with a type (e.g., [Type]$var)
            if (expr is ConvertExpressionAst convertExpr)
            {
                return convertExpr.Type.TypeName.FullName;
            }

            // Check if this is a variable expression
            if (expr is VariableExpressionAst varExpr)
            {
                // Walk up the AST to find if this variable has a type constraint in a parameter
                var parameterAst = FindParameterForVariable(varExpr);
                if (parameterAst != null)
                {
                    // Get the first type constraint attribute
                    var typeConstraint = parameterAst.Attributes
                        .OfType<TypeConstraintAst>()
                        .FirstOrDefault();

                    if (typeConstraint != null)
                    {
                        return typeConstraint.TypeName.FullName;
                    }
                }

                // Check if the variable was declared with a type constraint elsewhere
                // Look for assignment statements with type constraints
                var assignmentWithType = FindTypedAssignment(varExpr);
                if (assignmentWithType != null)
                {
                    return assignmentWithType;
                }
            }

            // Check if this is a member expression that might have a known return type
            // For now, we'll be conservative and only check direct type constraints

            return null;
        }

        /// <summary>
        /// Finds the parameter AST for a given variable expression, if it exists.
        /// </summary>
        private ParameterAst FindParameterForVariable(VariableExpressionAst varExpr)
        {
            if (varExpr == null)
            {
                return null;
            }

            var varName = varExpr.VariablePath.UserPath;

            // Walk up to find the containing function or script block
            Ast current = varExpr.Parent;
            while (current != null)
            {
                if (current is FunctionDefinitionAst funcAst)
                {
                    // Check parameters in the param block
                    var paramBlock = funcAst.Body?.ParamBlock;
                    if (paramBlock?.Parameters != null)
                    {
                        foreach (var param in paramBlock.Parameters)
                        {
                            if (string.Equals(param.Name.VariablePath.UserPath, varName, StringComparison.OrdinalIgnoreCase))
                            {
                                return param;
                            }
                        }
                    }

                    // Check function parameters (for functions with parameters outside param block)
                    if (funcAst.Parameters != null)
                    {
                        foreach (var param in funcAst.Parameters)
                        {
                            if (string.Equals(param.Name.VariablePath.UserPath, varName, StringComparison.OrdinalIgnoreCase))
                            {
                                return param;
                            }
                        }
                    }

                    break; // Don't check outer function scopes
                }

                if (current is ScriptBlockAst scriptAst)
                {
                    var paramBlock = scriptAst.ParamBlock;
                    if (paramBlock?.Parameters != null)
                    {
                        foreach (var param in paramBlock.Parameters)
                        {
                            if (string.Equals(param.Name.VariablePath.UserPath, varName, StringComparison.OrdinalIgnoreCase))
                            {
                                return param;
                            }
                        }
                    }
                    break; // Don't check outer script block scopes
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// Builds and caches typed variable assignments for a given scope.
        /// This is called once per scope to avoid O(N*M) performance issues.
        /// </summary>
        private Dictionary<string, string> GetOrBuildTypedVariableCache(Ast scope)
        {
            if (scope == null)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            // Check if we already have cached results for this scope
            if (_typedVariableCache.TryGetValue(scope, out var cachedResults))
            {
                return cachedResults;
            }

            // Build the cache for this scope
            var typedVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Find all assignment statements in this scope
            var assignments = scope.FindAll(testAst => testAst is AssignmentStatementAst, true);

            foreach (AssignmentStatementAst assignment in assignments)
            {
                // Check if the left side is a convert expression with a variable
                if (assignment.Left is ConvertExpressionAst convertExpr &&
                    convertExpr.Child is VariableExpressionAst assignedVar)
                {
                    var varName = assignedVar.VariablePath.UserPath;
                    var typeName = convertExpr.Type.TypeName.FullName;

                    // Store in cache (first assignment wins)
                    if (!typedVariables.ContainsKey(varName))
                    {
                        typedVariables[varName] = typeName;
                    }
                }
            }

            // Cache the results
            _typedVariableCache[scope] = typedVariables;
            return typedVariables;
        }

        /// <summary>
        /// Looks for a typed assignment to a variable using cached results.
        /// </summary>
        private string FindTypedAssignment(VariableExpressionAst varExpr)
        {
            if (varExpr == null)
            {
                return null;
            }

            var varName = varExpr.VariablePath.UserPath;

            // Walk up to find the containing function or script block
            Ast searchScope = varExpr.Parent;
            while (searchScope != null &&
                   !(searchScope is FunctionDefinitionAst) &&
                   !(searchScope is ScriptBlockAst))
            {
                searchScope = searchScope.Parent;
            }

            if (searchScope == null)
            {
                return null;
            }

            // Use cached results instead of re-scanning the entire scope
            var typedVariables = GetOrBuildTypedVariableCache(searchScope);

            if (typedVariables.TryGetValue(varName, out string typeName))
            {
                return typeName;
            }

            return null;
        }

        /// <summary>
        /// Checks module manifest (.psd1) files for CLM compatibility issues.
        /// </summary>
        private void CheckModuleManifest(Ast ast, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            // Find the hashtable in the manifest
            var hashtableAst = ast.Find(x => x is HashtableAst, false) as HashtableAst;

            if (hashtableAst == null)
            {
                return;
            }

            // Check for wildcard exports in FunctionsToExport, CmdletsToExport, AliasesToExport
            CheckWildcardExports(hashtableAst, fileName, diagnosticRecords);

            // Check for .ps1 files in RootModule, NestedModules, and ScriptsToProcess
            CheckScriptModules(hashtableAst, fileName, diagnosticRecords);
        }

        /// <summary>
        /// Checks for wildcard ('*') in export fields which are not allowed in CLM.
        /// </summary>
        private void CheckWildcardExports(HashtableAst hashtableAst, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            //AliasesToExport and VariablesToExport can use wildcards in CLM, but it is not recommended for performance reasons.
            string[] exportFields = { "FunctionsToExport", "CmdletsToExport"};

            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                if (kvp.Item1 is StringConstantExpressionAst keyAst)
                {
                    string keyName = keyAst.Value;

                    if (exportFields.Contains(keyName, StringComparer.OrdinalIgnoreCase))
                    {
                        // Check if the value contains a wildcard
                        bool hasWildcard = false;
                        IScriptExtent wildcardExtent = null;

                        // The value in a hashtable is a StatementAst, need to extract the expression
                        var valueExpr = GetExpressionFromStatement(kvp.Item2);

                        if (valueExpr is StringConstantExpressionAst stringValue)
                        {
                            if (stringValue.Value == "*")
                            {
                                hasWildcard = true;
                                wildcardExtent = stringValue.Extent;
                            }
                        }
                        else if (valueExpr is ArrayLiteralAst arrayValue)
                        {
                            foreach (var element in arrayValue.Elements)
                            {
                                if (element is StringConstantExpressionAst strElement && strElement.Value == "*")
                                {
                                    hasWildcard = true;
                                    wildcardExtent = strElement.Extent;
                                    break;
                                }
                            }
                        }
                        else if (valueExpr is ArrayExpressionAst arrayExpr)
                        {
                            // Array expressions like @('a', 'b') have a SubExpression inside
                            if (arrayExpr.SubExpression?.Statements != null)
                            {
                                foreach (var stmt in arrayExpr.SubExpression.Statements)
                                {
                                    var expr = GetExpressionFromStatement(stmt);
                                    if (expr is ArrayLiteralAst arrayLiteral)
                                    {
                                        foreach (var element in arrayLiteral.Elements)
                                        {
                                            if (element is StringConstantExpressionAst strElement && strElement.Value == "*")
                                            {
                                                hasWildcard = true;
                                                wildcardExtent = strElement.Extent;
                                                break;
                                            }
                                        }
                                    }
                                    else if (expr is StringConstantExpressionAst strElement && strElement.Value == "*")
                                    {
                                        // Handle single-item array expressions like @('*')
                                        hasWildcard = true;
                                        wildcardExtent = strElement.Extent;
                                        break;
                                    }
                                    if (hasWildcard) break;
                                }
                            }
                        }

                        if (hasWildcard && wildcardExtent != null)
                        {
                            diagnosticRecords.Add(
                                new DiagnosticRecord(
                                    String.Format(CultureInfo.CurrentCulture,
                                        Strings.UseConstrainedLanguageModeWildcardExportError,
                                        keyName),
                                    wildcardExtent,
                                    GetName(),
                                    GetDiagnosticSeverity(),
                                    fileName
                                ));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks for .ps1 files in RootModule, NestedModules, and ScriptsToProcess which are not recommended for CLM.
        /// </summary>
        private void CheckScriptModules(HashtableAst hashtableAst, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            string[] moduleFields = { "RootModule", "NestedModules", "ScriptsToProcess" };

            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                if (kvp.Item1 is StringConstantExpressionAst keyAst)
                {
                    string keyName = keyAst.Value;

                    if (moduleFields.Contains(keyName, StringComparer.OrdinalIgnoreCase))
                    {
                        var valueExpr = GetExpressionFromStatement(kvp.Item2);
                        CheckForPs1Files(valueExpr, keyName, fileName, diagnosticRecords);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts an ExpressionAst from a StatementAst (typically from hashtable values).
        /// </summary>
        private ExpressionAst GetExpressionFromStatement(StatementAst statement)
        {
            if (statement is PipelineAst pipeline && pipeline.PipelineElements.Count == 1)
            {
                if (pipeline.PipelineElements[0] is CommandExpressionAst commandExpr)
                {
                    return commandExpr.Expression;
                }
            }
            return null;
        }

        /// <summary>
        /// Helper method to get the appropriate error message for .ps1 file usage in module manifests.
        /// </summary>
        private string GetPs1FileErrorMessage(string fieldName, string scriptFileName)
        {
            if (fieldName.Equals("ScriptsToProcess", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(CultureInfo.CurrentCulture,
                    Strings.UseConstrainedLanguageModeScriptsToProcessError,
                    scriptFileName);
            }
            else
            {
                return String.Format(CultureInfo.CurrentCulture,
                    Strings.UseConstrainedLanguageModeScriptModuleError,
                    fieldName,
                    scriptFileName);
            }
        }

        /// <summary>
        /// Helper method to check if an expression contains .ps1 file references.
        /// </summary>
        private void CheckForPs1Files(ExpressionAst valueAst, string fieldName, string fileName, List<DiagnosticRecord> diagnosticRecords)
        {
            if (valueAst is StringConstantExpressionAst stringValue)
            {
                if (stringValue.Value != null && stringValue.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            GetPs1FileErrorMessage(fieldName, stringValue.Value),
                            stringValue.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }
            else if (valueAst is ArrayLiteralAst arrayValue)
            {
                foreach (var element in arrayValue.Elements)
                {
                    if (element is StringConstantExpressionAst strElement &&
                        strElement.Value != null &&
                        strElement.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                    {
                        diagnosticRecords.Add(
                            new DiagnosticRecord(
                                GetPs1FileErrorMessage(fieldName, strElement.Value),
                                strElement.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                fileName
                            ));
                    }
                }
            }
            else if (valueAst is ArrayExpressionAst arrayExpr)
            {
                // Array expressions like @('a', 'b') have a SubExpression inside
                if (arrayExpr.SubExpression?.Statements != null)
                {
                    foreach (var stmt in arrayExpr.SubExpression.Statements)
                    {
                        var expr = GetExpressionFromStatement(stmt);
                        if (expr is ArrayLiteralAst arrayLiteral)
                        {
                            foreach (var element in arrayLiteral.Elements)
                            {
                                if (element is StringConstantExpressionAst strElement &&
                                    strElement.Value != null &&
                                    strElement.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                                {
                                    diagnosticRecords.Add(
                                        new DiagnosticRecord(
                                            GetPs1FileErrorMessage(fieldName, strElement.Value),
                                            strElement.Extent,
                                            GetName(),
                                            GetDiagnosticSeverity(),
                                            fileName
                                        ));
                                }
                            }
                        }
                        else if (expr is StringConstantExpressionAst strElement &&
                            strElement.Value != null &&
                            strElement.Value.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                        {
                            diagnosticRecords.Add(
                                    new DiagnosticRecord(
                                        GetPs1FileErrorMessage(fieldName, strElement.Value),
                                        strElement.Extent,
                                        GetName(),
                                        GetDiagnosticSeverity(),
                                        fileName
                                    ));
                        }
                    }
                }
            }
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
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public override RuleSourceType SourceType => RuleSourceType.Builtin;
    }
}
