using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public abstract class CompatibilityRule : ConfigurableRule
    {
        private const string PROFILE_DIR_NAME = "compatibility_profiles";

        private static readonly string s_defaultProfileDirPath = Path.Combine(GetModuleRootDirPath(), PROFILE_DIR_NAME);

        private static readonly Regex s_falseProfileExtensionPattern = new Regex(
            "\\d+_(core|framework)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly CompatibilityProfileLoader _profileLoader;

        private readonly string _profileDirPath;

        public CompatibilityRule()
            : this(s_defaultProfileDirPath)
        {
        }

        public CompatibilityRule(string profileDirPath)
        {
            _profileDirPath = profileDirPath;
            _profileLoader = CompatibilityProfileLoader.StaticInstance;
        }

        /// <summary>
        /// The path to the "anyprofile union" profile.
        /// If given as a filename, this is presumed to be under the profiles directory.
        /// If no file extension is given on the filename, ".json" is assumed.
        /// </summary>
        /// <remarks>
        /// The default value for this should be PlatformNaming.AnyPlatformUnionName,
        /// but a non-constant expression cannot be used as an attribute parameter.
        /// This is done in the ConfigureRule() override below.
        /// The ConfigurableRuleProperty should just remove this parameter and use the
        /// property default value.
        /// </remarks>
        [ConfigurableRuleProperty(defaultValue: "")]
        public string AnyProfilePath { get; set; }

        [ConfigurableRuleProperty(defaultValue: new string[] {})]
        public string[] TargetProfilePaths { get; set; }

        public virtual DiagnosticSeverity DiagnosticSeverity => DiagnosticSeverity.Warning;

        protected abstract CompatibilityVisitor CreateVisitor(string fileName);

        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            CompatibilityVisitor compatibilityVisitor = CreateVisitor(fileName);
            ast.Visit(compatibilityVisitor);
            return compatibilityVisitor.GetDiagnosticRecords();
        }

        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);

            // Default anyprofile path is the one specified in the CrossCompatibility library
            if (string.IsNullOrEmpty(AnyProfilePath))
            {
                AnyProfilePath = PlatformNaming.AnyPlatformUnionName;
            }
        }

        protected Tuple<CompatibilityProfileData, CompatibilityProfileData[]> LoadCompatibilityProfiles()
        {
            if (string.IsNullOrEmpty(AnyProfilePath))
            {
                throw new InvalidOperationException($"{nameof(AnyProfilePath)} cannot be null or empty");
            }

            if (TargetProfilePaths == null)
            {
                throw new InvalidOperationException($"{nameof(TargetProfilePaths)} cannot be null");
            }

            if (TargetProfilePaths.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(TargetProfilePaths)} cannot be empty");
            }

            var targetProfiles = new List<CompatibilityProfileData>();
            foreach (string configPath in TargetProfilePaths)
            {
                string normalizedPath = NormalizeProfileNameToAbsolutePath(configPath);
                targetProfiles.Add(_profileLoader.GetProfileFromFilePath(normalizedPath));
            }

            CompatibilityProfileData anyProfile = _profileLoader.GetProfileFromFilePath(NormalizeProfileNameToAbsolutePath(AnyProfilePath));

            return new Tuple<CompatibilityProfileData, CompatibilityProfileData[]>(anyProfile, targetProfiles.ToArray());
        }

        private string NormalizeProfileNameToAbsolutePath(string profileName)
        {
            // Reject null or empty paths
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException($"{nameof(profileName)} cannot be null or empty");
            }

            // Accept absolute paths verbatim. There may be issues with paths like "/here" in Windows
            if (Path.IsPathRooted(profileName))
            {
                return profileName;
            }

            // Reject relative paths
            if (profileName.Contains("\\")
                || profileName.Contains("/")
                || profileName.Equals(".")
                || profileName.Equals(".."))
            {
                throw new ArgumentException($"Compatibility profile specified as '{profileName}'. Compatibility profiles cannot be specified by relative path.");
            }

            // Profiles might be given by pure name, in which case tack ".json" onto the end
            string extension = Path.GetExtension(profileName);
            if (string.IsNullOrEmpty(extension) || s_falseProfileExtensionPattern.IsMatch(extension))
            {
                profileName = profileName + ".json";
            }

            // Names get looked for in the known profile directory
            return Path.Combine(_profileDirPath, profileName);
        }

        private static string GetModuleRootDirPath()
        {
            string asmDirLocation = Path.GetDirectoryName(typeof(UseCompatibleCommands).Assembly.Location);

            string topDir = Path.GetFileName(asmDirLocation);

            string nonNormalizedRoot = "PSScriptAnalyzer".Equals(topDir, StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(asmDirLocation)
                : Path.Combine(asmDirLocation, "..");

            return Path.GetFullPath(nonNormalizedRoot);
        }
    }

    public abstract class CompatibilityVisitor : AstVisitor
    {
        public abstract IEnumerable<DiagnosticRecord> GetDiagnosticRecords();
    }

    public abstract class CompatibilityDiagnostic : DiagnosticRecord
    {
        protected CompatibilityDiagnostic(
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
            IEnumerable<CorrectionExtent> suggestedCorrections)
            : base(
                message,
                extent,
                ruleName,
                DiagnosticSeverity.Warning,
                analyzedFileName,
                ruleId: null,
                suggestedCorrections: suggestedCorrections)
        {
        }
    }
}