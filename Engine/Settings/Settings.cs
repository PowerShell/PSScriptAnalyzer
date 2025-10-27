// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{

    /// <summary>
    /// Central entry point for obtaining analyzer settings. Resolves the -Settings parameter
    /// (null, preset name, file path, or inline hashtable) into a SettingsData instance by:
    /// 1. Auto-discovering a settings file (PSScriptAnalyzerSettings.*) in the working directory.
    /// 2. Mapping preset names to shipped settings files (supporting multiple formats).
    /// 3. Loading and parsing settings files via registered format parsers (psd1, json).
    /// 4. Converting inline hashtables directly to SettingsData.
    /// Also exposes helpers to enumerate shipped presets and locate module resource folders.
    /// </summary>
    public static class Settings
    {

        private readonly static string DefaultSettingsFileName = "PSScriptAnalyzerSettings";

        /// <summary>
        /// Registered settings parsers in precedence order.
        /// The first matching parser "wins" for auto discovery and presets when multiple
        /// files of the same base name, but different supported extensions, exist.
        /// </summary>
        private static readonly List<ISettingsParser> s_parsers = new()
        {
            new JsonSettingsParser(),
            new Psd1SettingsParser()
        };

        /// <summary>
        /// Creates a <see cref="SettingsData"/> from a user-supplied Settings
        /// argument. Primarily used for testing.
        /// </summary>
        public static SettingsData Create(object settingsObj)
        {
            return Create(settingsObj, null, null);
        }

        /// <summary>
        /// Creates a <see cref="SettingsData"/> from a user-supplied -Settings argument.
        /// Accepted inputs:
        ///   null                -> attempt auto discovery in <paramref name="cwd"/>
        ///   string preset name  -> resolve shipped preset
        ///   string file path    -> load that file (psd1/json)
        ///   hashtable           -> inline settings
        /// Uses the PowerShell provider resolver if supplied to expand relative/wildcard paths.
        /// Returns null when no settings can be found (mode None).
        /// </summary>
        /// <param name="settingsObj">Hashtable, preset name, file path, or null.</param>
        /// <param name="cwd">Working directory for auto discovery (script or folder path).</param>
        /// <param name="outputWriter">An output writer.</param>
        /// <param name="pathResolver">Delegate from PSCmdlet to resolve provider paths (optional).</param>
        /// <returns>Populated <see cref="SettingsData"/> or null if none.</returns>
        internal static SettingsData Create(
            object settingsObj,
            string cwd,
            IOutputWriter outputWriter,
            PathResolver.GetResolvedProviderPathFromPSPath<string, ProviderInfo, Collection<string>> getResolvedProviderPathFromPSPathDelegate = null
        )
        {
            // Determine how we're being passed settings
            var result = ResolveSettingsSource(settingsObj, cwd, getResolvedProviderPathFromPSPathDelegate);

            return result.Kind switch
            {
                SettingsSourceKind.None => null,
                SettingsSourceKind.InlineHashtable => HashtableSettingsConverter.Convert(result.InlineHashtable),
                SettingsSourceKind.AutoFile or SettingsSourceKind.ExplicitFile or SettingsSourceKind.PresetFile => ParseFile(result.FilePath),
                _ => null,
            };

        }

        /// <summary>
        /// Intermediate resolution data describing where settings came from.
        /// For Kind InlineHashtable only InlineHashtable is populated; for file-based kinds
        /// FilePath is set.
        /// </summary>
        private struct ResolutionResult
        {
            public SettingsSourceKind Kind;
            public string FilePath;
            public Hashtable InlineHashtable;
        }

        /// <summary>
        /// Enumerates the distinct ways settings can be supplied or discovered.
        /// </summary>
        private enum SettingsSourceKind
        {
            /// <summary>No settings were provided and auto-discovery found nothing.</summary>
            None,
            /// <summary>Settings were discovered automatically from a file.</summary>
            AutoFile,
            /// <summary>Settings were explicitly provided via a file path.</summary>
            ExplicitFile,
            /// <summary>Settings were loaded from a preset file.</summary>
            PresetFile,
            /// <summary>Settings were provided inline as a hashtable.</summary>
            InlineHashtable
        }

        /// <summary>
        /// Resolves the source kind for the supplied settings object.
        /// Unwraps PSObject values to inspect the underlying CLR type.
        /// </summary>
        /// <param name="settingsObj">User -Settings argument value.</param>
        /// <param name="cwd">Working directory for auto discovery.</param>
        /// <returns>ResolutionResult indicating mode and relevant data.</returns>
        /// <exception cref="FileNotFoundException">Thrown when a string path does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown for unsupported input types.</exception>
        private static ResolutionResult ResolveSettingsSource(
            object settingsObj,
            string cwd,
            PathResolver.GetResolvedProviderPathFromPSPath<string, ProviderInfo, Collection<string>> getResolvedProviderPathFromPSPathDelegate = null
        )
        {
            // If we have no settings object, attempt auto-discovery of settings
            // file in the current working directory
            // If auto-discovery finds nothing, we return 'None'
            if (settingsObj == null)
            {
                var auto = TryAutoDiscover(cwd);
                return new ResolutionResult
                {
                    Kind = auto != null ? SettingsSourceKind.AutoFile : SettingsSourceKind.None,
                    FilePath = auto
                };
            }

            // Unwrap PSObject if necessary. Ensures we see the real underlying
            // type (string, Hashtable). Without this, everything passed as an
            // expression would remain a PSObject and fail the subsequent type
            // checks.
            if (settingsObj is PSObject pso)
            {
                settingsObj = pso.BaseObject;
            }

            if (settingsObj is Hashtable ht)
            {
                return new ResolutionResult
                {
                    Kind = SettingsSourceKind.InlineHashtable,
                    InlineHashtable = ht
                };
            }

            if (settingsObj is string s)
            {
                // Does the string correspond to a preset name?
                var presetPath = TryResolvePreset(s);
                if (presetPath != null)
                {
                    return new ResolutionResult
                    {
                        Kind = SettingsSourceKind.PresetFile,
                        FilePath = presetPath
                    };
                }

                // If it doesn't match a prefix, is it a valid file path?
                // Attempt provider path resolution if possible
                s = ResolveProviderPathIfPossible(s, getResolvedProviderPathFromPSPathDelegate);
                if (File.Exists(s))
                {
                    return new ResolutionResult
                    {
                        Kind = SettingsSourceKind.ExplicitFile,
                        FilePath = s
                    };
                }

                throw new FileNotFoundException($"Settings file '{s}' not found.");
            }

            throw new ArgumentException("Settings must be a hashtable, a preset name, or a file path.");
        }

        /// <summary>
        /// Attempts to locate a settings file in the supplied path's directory
        /// using the registered parser formats in precedence order.
        /// </summary>
        /// <param name="path">File or directory path.</param>
        /// <returns>Full path to discovered settings file or null.</returns>
        public static string TryAutoDiscover(string path)
        {
            // If no path provided, cannot auto-discover
            if (string.IsNullOrWhiteSpace(path)) return null;

            // If path is a file, get its directory
            string dir = path;
            if (File.Exists(dir))
            {
                dir = Path.GetDirectoryName(dir);
            }

            // If directory doesn't exist, cannot auto-discover
            if (!Directory.Exists(dir)) return null;

            // Test for the presence of a settings file for each of the formats
            // supported. The parsers format list determines precedence.
            foreach (var parser in s_parsers)
            {
                var filePath = Path.Combine(dir, $"{DefaultSettingsFileName}.{parser.FormatName}");
                if (File.Exists(filePath)) return filePath;
            }

            return null;
        }

        /// <summary>
        /// Resolves a shipped preset name to its settings file path.
        /// Searches supported formats in precedence order, returning the first match.
        /// </summary>
        /// <param name="name">Preset base name without extension.</param>
        /// <returns>Full file path or null if not found.</returns>
        public static string TryResolvePreset(string name)
        {
            // Get the path to the folder of preset settings files shipped with the module
            var settingsDir = GetShippedSettingsDirectory();

            // If we can't locate it, return null
            if (settingsDir == null) return null;

            // Loop through supported formats and check for existence
            // return the first match
            foreach (var parser in s_parsers)
            {
                var filePath = Path.Combine(settingsDir, name + "." + parser.FormatName);
                if (File.Exists(filePath)) return filePath;
            }

            return null;
        }

        /// <summary>
        /// Attempts provider path resolution (wildcards, PSDrive) using PSCmdlet delegate.
        /// Falls back to original path if resolution fails.
        /// </summary>
        /// <param name="path">Original path.</param>
        /// <param name="resolver">PSCmdlet resolution delegate.</param>
        /// <returns>Resolved single provider path or original path.</returns>
        private static string ResolveProviderPathIfPossible(
            string path,
            PathResolver.GetResolvedProviderPathFromPSPath<string, ProviderInfo, Collection<string>> getResolvedProviderPathFromPSPathDelegate)
        {
            if (getResolvedProviderPathFromPSPathDelegate == null) return path;
            try
            {
                var resolved = getResolvedProviderPathFromPSPathDelegate(path, out ProviderInfo _);
                if (resolved != null && resolved.Count == 1 && !string.IsNullOrEmpty(resolved[0]))
                {
                    return resolved[0];
                }
            }
            catch
            {
                // Ignore resolution errors; use original path.
            }
            return path;
        }

        /// <summary>
        /// Opens and parses the specified settings file using an appropriate registered parser.
        /// Clones result to stamp correct SourceKind if immutability prevents direct assignment.
        /// </summary>
        /// <param name="path">Existing settings file path.</param>
        /// <returns>Parsed <see cref="SettingsData"/>.</returns>
        /// <exception cref="NotSupportedException">If no parser can handle the file.</exception>
        private static SettingsData ParseFile(string path)
        {
            var parser = s_parsers.Find(p => p.CanParse(path)) ??
                throw new NotSupportedException($"No parser registered for settings file '{path}'.");
            using var fs = File.OpenRead(path);
            var data = parser.Parse(fs, path);
            return data;
        }

        /// <summary>
        /// Retrieves the Settings directory from the Module directory structure
        /// </summary>
        /// <returns>The Settings directory path</returns>
        public static string GetShippedSettingsDirectory()
        {
            // Find the compatibility files in Settings folder
            var path = typeof(Helper).GetTypeInfo().Assembly.Location;
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // Find the compatibility files in Settings folder adjacent to the assembly.
            // Some builds place binaries in subfolders (coreclr/, PSv3/); in those cases,
            // the Settings folder lives in the parent (module root), so we also probe one level up.
            var settingsPath = Path.Combine(Path.GetDirectoryName(path), "Settings");
            if (!Directory.Exists(settingsPath))
            {
                // Probe parent directory (module root) for Settings folder.
                var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(path));
                settingsPath = Path.Combine(parentDir ?? string.Empty, "Settings");
                if (!Directory.Exists(settingsPath))
                {
                    return null;
                }
            }

            return settingsPath;
        }

        /// <summary>
        /// Retrieves the Settings directory from the Module directory structure
        /// </summary>
        /// <returns>The Settings directory path</returns>
        public static string GetShippedCommandDataFileDirectory()
        {
            // Find the compatibility files in Settings folder
            var path = typeof(Helper).GetTypeInfo().Assembly.Location;
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // Find the compatibility files in Settings folder adjacent to the assembly.
            // Some builds place binaries in subfolders (coreclr/, PSv3/); in those cases,
            // the Settings folder lives in the parent (module root), so we also probe one level up.
            var commandDataFilesPath = Path.Combine(Path.GetDirectoryName(path), "CommandDataFiles");
            if (!Directory.Exists(commandDataFilesPath))
            {
                // Probe parent directory (module root) for CommandDataFiles folder.
                var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(path));
                commandDataFilesPath = Path.Combine(parentDir ?? string.Empty, "CommandDataFiles");
                if (!Directory.Exists(commandDataFilesPath))
                {
                    return null;
                }
            }

            return commandDataFilesPath;
        }

        /// <summary>
        /// Returns the builtin setting presets
        ///
        /// Looks for supported formats in the PSScriptAnalyzer module settings directory
        /// and returns the names of the files without extension
        /// </summary>
        public static IEnumerable<string> GetSettingPresets()
        {
            var settingsPath = GetShippedSettingsDirectory();

            if (settingsPath == null)
            {
                yield break;
            }

            // Collect unique preset base names across all supported formats.
            // e.g. if both Psd1 and Json versions of the same preset exist,
            // only yield the name once.
            var yielded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var parser in s_parsers)
            {
                var pattern = "*." + parser.FormatName;
                foreach (var filepath in Directory.EnumerateFiles(settingsPath, pattern))
                {
                    var name = Path.GetFileNameWithoutExtension(filepath);
                    if (yielded.Add(name))
                    {
                        yield return name;
                    }
                }
            }
        }

    }

}