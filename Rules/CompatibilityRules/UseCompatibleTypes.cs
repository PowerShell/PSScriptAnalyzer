// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{

    /// <summary>
    /// Rule to check that .NET type usage in PowerShell is compatible
    /// with configured target PowerShell runtimes.
    /// </summary>
#if !CORECLR
    [System.ComponentModel.Composition.Export(typeof(IScriptRule))]
#endif
    public class UseCompatibleTypes : CompatibilityRule
    {
        /// <summary>
        /// Get the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesCommonName);
        }

        /// <summary>
        /// Get the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesDescription);
        }

        /// <summary>
        /// Get the localized name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseCompatibleTypesName);
        }

        /// <summary>
        /// Create a visitor to check type compatibility of a PowerShell AST.
        /// </summary>
        /// <param name="fileName">The path of the PowerShell script to be checked.</param>
        /// <returns>An AST visitor to perform type compatibility analysis.</returns>
        protected override CompatibilityVisitor CreateVisitor(string fileName)
        {
            IEnumerable<CompatibilityProfileData> targetProfiles = LoadCompatibilityProfiles(out CompatibilityProfileData unionProfile);
            return new TypeCompatibilityVisitor(fileName, unionProfile, targetProfiles, this);
        }

        private class TypeCompatibilityVisitor : CompatibilityVisitor
        {
            private readonly string _analyzedFileName;

            private readonly CompatibilityProfileData _anyProfile;

            private readonly IEnumerable<CompatibilityProfileData> _compatibilityTargets;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly UseCompatibleTypes _rule;

            public TypeCompatibilityVisitor(
                string analyzedFileName,
                CompatibilityProfileData anyProfile,
                IEnumerable<CompatibilityProfileData> compatibilityTargetProfiles,
                UseCompatibleTypes rule)
            {
                _analyzedFileName = analyzedFileName;
                _anyProfile = anyProfile;
                _compatibilityTargets = compatibilityTargetProfiles;
                _rule = rule;
                _diagnosticAccumulator = new List<DiagnosticRecord>();
            }

            public override IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }

            public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
            {
                TryFindTypeIncompatibilities(typeExpressionAst.TypeName);
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
            {
                TryFindTypeIncompatibilities(typeConstraintAst.TypeName);
                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
            {
                if (attributeAst.TypeName != null)
                {
                    TryFindTypeIncompatibilities(attributeAst.TypeName);
                }

                return AstVisitAction.SkipChildren;
            }

            public override AstVisitAction VisitCommand(CommandAst commandAst)
            {
                string commandName = commandAst?.GetCommandName();
                if (commandName == null)
                {
                    return AstVisitAction.Continue;
                }

                if (commandName.Equals("New-Object"))
                {
                    if (!TryGetArgument(commandAst.CommandElements, 0, "TypeName", out CommandElementAst typeNameArg))
                    {
                        return AstVisitAction.Continue;
                    }

                    if (!(typeNameArg is StringConstantExpressionAst typeNameStringExp))
                    {
                        return AstVisitAction.Continue;
                    }

                    // We need the PowerShell parser to turn this into something structured now
                    TypeExpressionAst typeNameAst = (TypeExpressionAst)Parser.ParseInput("["+typeNameStringExp.Value+"]", out Token[] tokens, out ParseError[] errors)
                        .Find(ast => ast is TypeExpressionAst, true);

                    if (typeNameAst == null)
                    {
                        return AstVisitAction.Continue;
                    }

                    TryFindTypeIncompatibilities(typeNameAst.TypeName);
                    return AstVisitAction.Continue;
                }

                return AstVisitAction.Continue;
            }

            public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
            {
                // Look for static method invocations not supported
                if (!methodCallAst.Static)
                {
                    return AstVisitAction.Continue;
                }

                if (!(methodCallAst.Expression is TypeExpressionAst typeExpr))
                {
                    return AstVisitAction.Continue;
                }

                if (!(methodCallAst.Member is StringConstantExpressionAst methodNameAst))
                {
                    return AstVisitAction.Continue;
                }

                // We only need to get the full type name once -- from the union profile
                string typeName = TypeNaming.GetOuterMostTypeName(_anyProfile.Runtime.Types.TypeAcceleratorNames, typeExpr.TypeName);
                if (!_anyProfile.Runtime.Types.Types.TryGetValue(typeName, out TypeData unionType))
                {
                    return AstVisitAction.Continue;
                }

                if (!unionType.Static.Methods.ContainsKey(methodNameAst.Value))
                {
                    return AstVisitAction.Continue;
                }

                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    if (!targetProfile.Runtime.Types.Types.TryGetValue(typeName, out TypeData typeData))
                    {
                        continue;
                    }

                    if (typeData.Static.Methods.ContainsKey(methodNameAst.Value))
                    {
                        continue;
                    }

                    _diagnosticAccumulator.Add(TypeCompatibilityDiagnostic.CreateForStaticMethod(
                        typeName,
                        methodNameAst.Value,
                        targetProfile.Platform,
                        methodCallAst.Extent,
                        _analyzedFileName,
                        _rule));
                }

                return AstVisitAction.Continue;
            }

            public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
            {
                // Can only check static members
                if (!memberExpressionAst.Static)
                {
                    return AstVisitAction.Continue;
                }

                // Need to make sure the calling expression is a TypeExpressionAst (this should be ensured by the IsStatic)
                if (!(memberExpressionAst.Expression is TypeExpressionAst typeExpressionAst))
                {
                    return AstVisitAction.Continue;
                }

                // Need a static constant member name
                if (!(memberExpressionAst.Member is StringConstantExpressionAst memberNameAst))
                {
                    return AstVisitAction.Continue;
                }

                // Get the outer type and make sure it exists in the anyProfile
                string typeName = TypeNaming.GetOuterMostTypeName(_anyProfile.Runtime.Types.TypeAcceleratorNames, typeExpressionAst.TypeName);
                if (!_anyProfile.Runtime.Types.Types.TryGetValue(typeName, out TypeData unionType))
                {
                    return AstVisitAction.Continue;
                }

                // Make sure the member also exists on the type from the anyProfile, and check what kind of member it is
                bool isProperty = unionType.Static.Properties.ContainsKey(memberNameAst.Value);
                bool isEvent = !isProperty && unionType.Static.Events.ContainsKey(memberNameAst.Value);
                bool isField = !isEvent && unionType.Static.Fields.ContainsKey(memberNameAst.Value);
                if (!isField)
                {
                    return AstVisitAction.Continue;
                }

                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    if (!targetProfile.Runtime.Types.Types.TryGetValue(typeName, out TypeData profileType))
                    {
                        continue;
                    }

                    if (isProperty && profileType.Static.Properties.ContainsKey(memberNameAst.Value))
                    {
                        continue;
                    }

                    if (isEvent && profileType.Static.Events.ContainsKey(memberNameAst.Value))
                    {
                        continue;
                    }

                    if (isField && profileType.Static.Fields.ContainsKey(memberNameAst.Value))
                    {
                        continue;
                    }

                    _diagnosticAccumulator.Add(TypeCompatibilityDiagnostic.CreateForStaticProperty(
                        typeName,
                        memberNameAst.Value,
                        targetProfile.Platform,
                        memberExpressionAst.Extent,
                        _analyzedFileName,
                        _rule));
                }

                return AstVisitAction.Continue;
            }

            /// <summary>
            /// Try to get an argument with a given position/parameter name in a PowerShell command call.
            /// </summary>
            /// <param name="commandElements">The full list of command elements in the call.</param>
            /// <param name="argumentPosition">The position of the argument (starting from 0).</param>
            /// <param name="argumentParameterName">The name of the parameter of the argument.</param>
            /// <param name="argument">If the argument is found, the AST where its value is specified.</param>
            /// <returns>True if the argument was found, false otherwise.</returns>
            /// <remarks>
            /// This implementation is simplified to ignore the positions of parameters
            /// that have a position but are given by name, and may be incorrect.
            /// Ideally it should use parameter information for the command to work out the binding.
            /// </remarks>
            private bool TryGetArgument(IReadOnlyList<CommandElementAst> commandElements, int argumentPosition, string argumentParameterName, out CommandElementAst argument)
            {
                argumentPosition++;
                int effectivePosition = 1;
                bool sawParameterName = false;
                bool expectingRequiredArgument = false;
                for (int i = 1; i < commandElements.Count; i++)
                {
                    CommandElementAst commandElement = commandElements[i];

                    // See a parameter, like -TypeName
                    if (commandElement is CommandParameterAst parameter)
                    {
                        // We were expecting an argument value, but the previous parameter was used like a switch
                        if (expectingRequiredArgument)
                        {
                            argument = null;
                            return false;
                        }

                        // We've seen a parameter name, and now set up to see an argument
                        sawParameterName = true;
                        if (parameter.ParameterName.Equals(argumentParameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            expectingRequiredArgument = true;
                        }

                        // TODO:
                        // To get positional parameters correct, we need to know
                        // what the position of the named parameter was so that we
                        // can skip over that if we come to it

                        continue;
                    }

                    // We saw the parameter we were looking for and here is the argument
                    if (expectingRequiredArgument)
                    {
                        argument = commandElement;
                        return true;
                    }

                    // This is some other parameter we're not interested in
                    if (sawParameterName)
                    {
                        sawParameterName = false;
                        continue;
                    }

                    // We have found the argument by position, not name
                    if (effectivePosition == argumentPosition)
                    {
                        argument = commandElement;
                        return true;
                    }

                    effectivePosition++;
                }

                argument = null;
                return false;
            }

            private bool TryFindTypeIncompatibilities(ITypeName typeName)
            {
                switch (typeName)
                {
                    case ArrayTypeName arrayTypeName:
                        return TryFindTypeIncompatibilities(arrayTypeName.ElementType);

                    case GenericTypeName genericTypeName:
                        bool hasIncompatibility = TryFindTypeIncompatibilities(genericTypeName.TypeName);
                        foreach (ITypeName genericArg in genericTypeName.GenericArguments)
                        {
                            hasIncompatibility |= TryFindTypeIncompatibilities(genericArg);
                        }
                        return hasIncompatibility;
                }

                return TryFindTypeIncompatibilities(typeName.FullName, typeName.Extent);
            }

            private bool TryFindTypeIncompatibilities(string typeName, IScriptExtent extent)
            {
                if (IsTypeAcceleratorUnsupportedByTarget(typeName, out IReadOnlyList<PlatformData> unsupportedTypeAcceleratorTargets))
                {
                    foreach (PlatformData target in unsupportedTypeAcceleratorTargets)
                    {
                        var diagnostic = TypeCompatibilityDiagnostic.CreateForTypeAccelerator(
                            typeName,
                            target,
                            extent,
                            _analyzedFileName,
                            _rule);

                        _diagnosticAccumulator.Add(diagnostic);
                    }

                    return true;
                }

                if (IsTypeUnsupportedByTarget(typeName, out IReadOnlyList<PlatformData> unsupportedTypeTargets))
                {
                    foreach (PlatformData target in unsupportedTypeTargets)
                    {
                        var diagnostic = TypeCompatibilityDiagnostic.CreateForType(
                            typeName,
                            target,
                            extent,
                            _analyzedFileName,
                            _rule);

                        _diagnosticAccumulator.Add(diagnostic);
                    }

                    return true;
                }

                return false;
            }

            private bool IsTypeAcceleratorUnsupportedByTarget(string typeName, out IReadOnlyList<PlatformData> unsupportedTargets)
            {
                // No known type accelerators contain "."
                if (typeName.Contains("."))
                {
                    unsupportedTargets = null;
                    return false;
                }

                if (!_anyProfile.Runtime.Types.TypeAccelerators.ContainsKey(typeName))
                {
                    unsupportedTargets = null;
                    return false;
                }

                var targetsWithoutTypeAccelerator = new List<PlatformData>();
                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    if (!targetProfile.Runtime.Types.TypeAccelerators.ContainsKey(typeName))
                    {
                        targetsWithoutTypeAccelerator.Add(targetProfile.Platform);
                    }
                }

                if (targetsWithoutTypeAccelerator.Count > 0)
                {
                    unsupportedTargets = targetsWithoutTypeAccelerator;
                    return true;
                }

                unsupportedTargets = null;
                return false;
            }

            private bool IsTypeUnsupportedByTarget(string typeName, out IReadOnlyList<PlatformData> unsupportedTargets)
            {
                string canonicalAnyProfileTypeName = TypeNaming.ExpandSimpleTypeName(_anyProfile.Runtime.Types.TypeAcceleratorNames, typeName);

                string systemExtendedTypeName = null;
                if (!_anyProfile.Runtime.Types.Types.ContainsKey(canonicalAnyProfileTypeName))
                {
                    // PowerShell will implicitly add "System." and try that too,
                    // so we must check both
                    systemExtendedTypeName = "System." + canonicalAnyProfileTypeName;
                    if (!_anyProfile.Runtime.Types.Types.ContainsKey(systemExtendedTypeName))
                    {
                        // Neither the type nor "system.<type>" exist in the any profile, so no diagnostic
                        unsupportedTargets = null;
                        return false;
                    }
                }

                string typeNameToCheck = systemExtendedTypeName ?? canonicalAnyProfileTypeName;
                var incompatiblePlatforms = new List<PlatformData>();
                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    if (!targetProfile.Runtime.Types.Types.ContainsKey(typeNameToCheck))
                    {
                        incompatiblePlatforms.Add(targetProfile.Platform);
                    }
                }

                if (incompatiblePlatforms.Count > 0)
                {
                    unsupportedTargets = incompatiblePlatforms;
                    return true;
                }

                unsupportedTargets = null;
                return false;
            }
        }
    }

    /// <summary>
    /// A diagnostic indicating an incompatibility of a type used
    /// with a given PowerShell target runtime.
    /// </summary>
    public class TypeCompatibilityDiagnostic : CompatibilityDiagnostic
    {
        /// <summary>
        /// Create a new type incompatibility diagnostic
        /// for a type accelerator.
        /// </summary>
        /// <param name="typeAcceleratorName">The name of the type accelerator used.</param>
        /// <param name="platform">The PowerShell platform where the type accelerator is incompatible.</param>
        /// <param name="extent">The AST extent of the offending type accelerator.</param>
        /// <param name="analyzedFileName">The path to the script being analyzed.</param>
        /// <param name="rule">The type compatibility rule.</param>
        /// <param name="suggestedCorrections">Any suggested corrections to fix the problem.</param>
        /// <returns></returns>
        public static TypeCompatibilityDiagnostic CreateForTypeAccelerator(
            string typeAcceleratorName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = String.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleTypesTypeAcceleratorError,
                typeAcceleratorName,
                platform.PowerShell.Version,
                platform.OperatingSystem.Name);

            return new TypeCompatibilityDiagnostic(
                typeAcceleratorName,
                platform,
                message,
                extent,
                rule.GetName(),
                ruleId: null,
                analyzedFileName: analyzedFileName,
                kind: TypeCompatibilityDiagnosticKind.TypeAccelerator,
                suggestedCorrections: suggestedCorrections);
        }

        /// <summary>
        /// Create a PowerShell type incompatibility diagnostic
        /// for a type.
        /// </summary>
        /// <param name="typeName">The full name of the type that is incompatible.</param>
        /// <param name="platform">The PowerShell platform where the type is incompatible.</param>
        /// <param name="extent">The AST extent where the incompatible type appears.</param>
        /// <param name="analyzedFileName">The path of the script being analyzed.</param>
        /// <param name="rule">The type incompatibility rule generating the diagnostic.</param>
        /// <param name="suggestedCorrections">Any suggested replacements in the script to prevent the incompatibility.</param>
        /// <returns></returns>
        public static TypeCompatibilityDiagnostic CreateForType(
            string typeName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = String.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleTypesTypeError,
                typeName,
                platform.PowerShell.Version,
                platform.OperatingSystem.Name);

            return new TypeCompatibilityDiagnostic(
                typeName,
                platform,
                message,
                extent,
                rule.GetName(),
                ruleId: null,
                analyzedFileName: analyzedFileName,
                kind: TypeCompatibilityDiagnosticKind.Type,
                suggestedCorrections: suggestedCorrections);
        }

        /// <summary>
        /// Create a PowerShell type incompatibility diagnostic
        /// for a static method invocation.
        /// </summary>
        /// <param name="typeName">The full name of the type where the incompatibility occurs.</param>
        /// <param name="methodName">The name of the method that is incompatible.</param>
        /// <param name="platform">The PowerShell platform where the type is incompatible.</param>
        /// <param name="extent">The AST extent where the incompatible type appears.</param>
        /// <param name="analyzedFileName">The path of the script being analyzed.</param>
        /// <param name="rule">The type incompatibility rule generating the diagnostic.</param>
        /// <param name="suggestedCorrections">Any suggested replacements in the script to prevent the incompatibility.</param>
        /// <returns></returns>
        public static TypeCompatibilityDiagnostic CreateForStaticMethod(
            string typeName,
            string methodName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = String.Format(
                CultureInfo.CurrentCulture,
                "The method '{0}' on type '{1}' is not available in PowerShell {2} on platform '{3}'",
                methodName,
                typeName,
                platform.PowerShell.Version,
                platform.OperatingSystem.FriendlyName);

            return new TypeCompatibilityDiagnostic(
                typeName,
                platform,
                message,
                extent,
                rule.GetName(),
                memberName: methodName,
                ruleId: null,
                analyzedFileName: analyzedFileName,
                kind: TypeCompatibilityDiagnosticKind.Type,
                suggestedCorrections: suggestedCorrections);
        }

        /// <summary>
        /// Create a PowerShell type incompatibility diagnostic
        /// for a static property usage.
        /// </summary>
        /// <param name="typeName">The full name of the type where the incompatibility occurs.</param>
        /// <param name="propertyName">The name of the property that is incompatible.</param>
        /// <param name="platform">The PowerShell platform where the type is incompatible.</param>
        /// <param name="extent">The AST extent where the incompatible type appears.</param>
        /// <param name="analyzedFileName">The path of the script being analyzed.</param>
        /// <param name="rule">The type incompatibility rule generating the diagnostic.</param>
        /// <param name="suggestedCorrections">Any suggested replacements in the script to prevent the incompatibility.</param>
        /// <returns></returns>
        public static TypeCompatibilityDiagnostic CreateForStaticProperty(
            string typeName,
            string propertyName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = String.Format(
                CultureInfo.CurrentCulture,
                "The member '{0}' on type '{1}' is not available in PowerShell {2} on platform '{3}'",
                propertyName,
                typeName,
                platform.PowerShell.Version,
                platform.OperatingSystem.FriendlyName);

            return new TypeCompatibilityDiagnostic(
                typeName,
                platform,
                message,
                extent,
                rule.GetName(),
                memberName: propertyName,
                ruleId: null,
                analyzedFileName: analyzedFileName,
                kind: TypeCompatibilityDiagnosticKind.Type,
                suggestedCorrections: suggestedCorrections);
        }

        private TypeCompatibilityDiagnostic(
            string incompatibleCommand,
            PlatformData targetPlatform,
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
            TypeCompatibilityDiagnosticKind kind,
            string memberName = null,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
            : base(
                message,
                extent,
                ruleName,
                ruleId,
                analyzedFileName,
                suggestedCorrections)
        {
            Type = incompatibleCommand;
            TargetPlatform = targetPlatform;
            MemberName = memberName;
            Kind = kind;
        }

        /// <summary>
        /// The full name of the type or type accelerator that is incompatible.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The PowerShell platform where the type would be incompatible.
        /// </summary>
        public PlatformData TargetPlatform { get; }

        /// <summary>
        /// Denotes what kind of type diagnostic this is.
        /// </summary>
        public TypeCompatibilityDiagnosticKind Kind { get; }

        /// <summary>
        /// If this diagnostic is about a type member, the name of that member.
        /// </summary>
        public string MemberName { get; }
    }

    /// <summary>
    /// Label for the different kinds of type compatibility diagnostic that may occur
    /// </summary>
    public enum TypeCompatibilityDiagnosticKind
    {
        /// <summary>Denotes a type compatibility diagnostic about the usage of a type.</summary>
        Type,
        /// <summary>Denotes a type compatibility diagnostic about the usage of a type accelerator.</summary>
        TypeAccelerator,
        /// <summary>Denotes a type compatibility diagnostic about the usage of a static method.</summary>
        StaticMethod,
        /// <summary>Denotes a type compatibility diagnostic about the usage of a static property.</summary>
        StaticProperty
    }
}