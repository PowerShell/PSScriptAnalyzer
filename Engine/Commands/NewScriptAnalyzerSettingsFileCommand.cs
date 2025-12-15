using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// Creates a new PSScriptAnalyzer settings file in the specified directory
    /// optionally based on a preset, a blank template, or all rules with default arguments.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ScriptAnalyzerSettingsFile", SupportsShouldProcess = true)]
    [OutputType(typeof(string))]
    public sealed class NewScriptAnalyzerSettingsFileCommand : PSCmdlet, IOutputWriter
    {
        private const string BaseOption_All = "All";
        private const string BaseOption_Blank = "Blank";

        /// <summary>
        /// Target directory (or file path) where the settings file will be created. Defaults to
        /// current location.
        /// </summary>
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Settings file format/extension (e.g. json, psd1). Defaults to first supported format.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(FileFormatCompleter))]
        [ValidateNotNullOrEmpty]
        public string FileFormat { get; set; }

        /// <summary>
        /// Base content: 'Blank', 'All', or a preset name returned by Get-SettingPresets.
        /// 'Blank' -> minimal empty settings.
        /// 'All'   -> include all rules and their configurable arguments with current defaults.
        /// preset  -> copy preset contents.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(SettingsBaseCompleter))]
        [ValidateNotNullOrEmpty]
        public string Base { get; set; } = BaseOption_Blank;

        /// <summary>
        /// Overwrite existing file if present.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing()
        {
            Helper.Instance = new Helper(SessionState.InvokeCommand);
            Helper.Instance.Initialize();

            string[] rulePaths = Helper.ProcessCustomRulePaths(null, SessionState, false);
            ScriptAnalyzer.Instance.Initialize(this, rulePaths, null, null, null, null == rulePaths);
        }

        protected override void ProcessRecord()
        {
            // Default Path
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = SessionState.Path.CurrentFileSystemLocation.ProviderPath;
            }

            // If user passed an existing file path, switch to its directory.
            if (File.Exists(Path))
            {
                Path = System.IO.Path.GetDirectoryName(Path);
            }

            // Require the directory to already exist (do not create it).
            if (!Directory.Exists(Path))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new DirectoryNotFoundException($"Directory '{Path}' does not exist."),
                    "DIRECTORY_NOT_FOUND",
                    ErrorCategory.ObjectNotFound,
                    Path));
                return;
            }

            // Ensure FileSystem provider for target Path.
            ProviderInfo providerInfo;
            try
            {
                SessionState.Path.GetResolvedProviderPathFromPSPath(Path, out providerInfo);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot resolve path '{Path}': {ex.Message}", ex),
                    "PATH_RESOLVE_FAILED",
                    ErrorCategory.InvalidArgument,
                    Path));
                return;
            }

            if (!string.Equals(providerInfo.Name, "FileSystem", StringComparison.OrdinalIgnoreCase))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Target path must be in the FileSystem provider."),
                    "INVALID_PROVIDER",
                    ErrorCategory.InvalidArgument,
                    Path));
            }

            // Default format to first supported.
            if (string.IsNullOrWhiteSpace(FileFormat))
            {
                FileFormat = Settings.GetSettingsFormats().First();
            }

            // Validate requested format.
            if (!Settings.GetSettingsFormats().Any(f => string.Equals(f, FileFormat, StringComparison.OrdinalIgnoreCase)))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException($"Unsupported settings format '{FileFormat}'."),
                    "UNSUPPORTED_FORMAT",
                    ErrorCategory.InvalidArgument,
                    FileFormat));
            }

            var targetFile = System.IO.Path.Combine(Path, $"{Settings.DefaultSettingsFileName}.{FileFormat}");

            if (File.Exists(targetFile) && !Force)
            {
                WriteWarning($"Settings file already exists: {targetFile}. Use -Force to overwrite.");
                return;
            }

            SettingsData data;
            try
            {
                data = BuildSettingsData();
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "BUILD_SETTINGS_FAILED",
                    ErrorCategory.InvalidData,
                    Base));
                return;
            }

            string content;
            try
            {
                content = Settings.Serialize(data, FileFormat);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "SERIALIZE_FAILED",
                    ErrorCategory.InvalidData,
                    FileFormat));
                return;
            }

            if (ShouldProcess(targetFile, "Create settings file"))
            {
                try
                {
                    File.WriteAllText(targetFile, content);
                    WriteVerbose($"Created settings file: {targetFile}");
                }
                catch (Exception ex)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        ex,
                        "CREATE_FILE_FAILED",
                        ErrorCategory.InvalidData,
                        targetFile));
                    return;
                }
                WriteObject(targetFile);
            }
        }

        private SettingsData BuildSettingsData()
        {
            if (string.Equals(Base, BaseOption_Blank, StringComparison.OrdinalIgnoreCase))
            {
                return new SettingsData(); // empty snapshot
            }

            if (string.Equals(Base, BaseOption_All, StringComparison.OrdinalIgnoreCase))
            {
                return BuildAllSettingsData();
            }

            // Preset
            var presetPath = Settings.TryResolvePreset(Base);
            if (presetPath == null)
            {
                throw new FileNotFoundException($"Preset '{Base}' not found.");
            }
            return Settings.Create(presetPath);
        }

        private SettingsData BuildAllSettingsData()
        {
            var ruleNames = new List<string>();
            var ruleArgs = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            var modNames = ScriptAnalyzer.Instance.GetValidModulePaths();
            var rules = ScriptAnalyzer.Instance.GetRule(modNames, null) ?? Enumerable.Empty<IRule>();

            foreach (var rule in rules)
            {
                var name = rule.GetName();
                ruleNames.Add(name);

                if (rule is ConfigurableRule configurable)
                {
                    var props = rule.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var argDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in props)
                    {
                        if (p.GetCustomAttribute<ConfigurableRulePropertyAttribute>(inherit: true) == null)
                        {
                            continue;
                        }
                        argDict[p.Name] = p.GetValue(rule);
                    }
                    if (argDict.Count > 0)
                    {
                        ruleArgs[name] = argDict;
                    }
                }
            }

            return new SettingsData
            {
                IncludeRules = ruleNames,
                RuleArguments = ruleArgs,
            };
        }

        #region Completers

        private sealed class FileFormatCompleter : IArgumentCompleter
        {
            public IEnumerable<CompletionResult> CompleteArgument(string commandName,
                string parameterName, string wordToComplete, CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                foreach (var fmt in Settings.GetSettingsFormats())
                {
                    if (fmt.StartsWith(wordToComplete ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new CompletionResult(fmt, fmt, CompletionResultType.ParameterValue, $"Settings format '{fmt}'");
                    }
                }
            }
        }

        private sealed class SettingsBaseCompleter : IArgumentCompleter
        {
            public IEnumerable<CompletionResult> CompleteArgument(string commandName,
                string parameterName, string wordToComplete, CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                var bases = new List<string> { BaseOption_Blank, BaseOption_All };
                bases.AddRange(Settings.GetSettingPresets());

                foreach (var b in bases)
                {
                    if (b.StartsWith(wordToComplete ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new CompletionResult(b, b, CompletionResultType.ParameterValue, $"Base template '{b}'");
                    }
                }
            }
        }

        #endregion
    }
}