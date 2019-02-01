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
    public class UseCompatibleTypes : CompatibilityRule
    {
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesCommonName);
        }

        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesDescription);
        }

        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesName);
        }

        protected override CompatibilityVisitor CreateVisitor(string fileName)
        {
            Tuple<CompatibilityProfileData, CompatibilityProfileData[]> profiles = LoadCompatibilityProfiles();
            return new TypeCompatibilityVisitor(fileName, profiles.Item1, profiles.Item2, this);
        }

        private class TypeCompatibilityVisitor : CompatibilityVisitor
        {
            private readonly string _analyzedFileName;

            private readonly CompatibilityProfileData _anyProfile;

            private readonly IReadOnlyList<CompatibilityProfileData> _compatibilityTargets;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly UseCompatibleTypes _rule;

            public TypeCompatibilityVisitor(
                string analyzedFileName,
                CompatibilityProfileData anyProfile,
                IReadOnlyList<CompatibilityProfileData> compatibilityTargetProfiles,
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

            public override AstVisitAction VisitCommand(CommandAst commandAst)
            {
                string commandName = commandAst?.GetCommandName();
                if (commandName == null)
                {
                    return AstVisitAction.SkipChildren;
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

                    TryFindTypeIncompatibilities(typeNameStringExp.Value, typeNameStringExp.Extent);
                    return AstVisitAction.Continue;
                }

                return AstVisitAction.Continue;
            }

            private bool TryGetArgument(IReadOnlyList<CommandElementAst> commandElements, int argumentPosition, string argumentParameterName, out CommandElementAst argument)
            {
                int effectivePosition = 0;
                bool sawParameterName = false;
                bool expectingRequiredArgument = false;
                for (int i = 0; i < commandElements.Count; i++)
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
                string canonicalAnyProfileTypeName = TypeNaming.GetCanonicalTypeName(_anyProfile.Runtime.Types.TypeAcceleratorNames, typeName);

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

    public class TypeCompatibilityDiagnostic : CompatibilityDiagnostic
    {
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
                Strings.UseCompatibleTypesAcceleratorError,
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
                suggestedCorrections: suggestedCorrections);
        }

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
            bool isTypeAccelerator = false,
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
            IsTypeAccelerator = isTypeAccelerator;
        }

        public string Type { get; }

        public PlatformData TargetPlatform { get; }

        public bool IsTypeAccelerator { get; }
    }
}