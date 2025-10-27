// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides an interface for a settings parser.
    /// </summary>
    public interface ISettingsParser
    {
        /// <summary>
        /// Gets the name of the format this parser supports. e.g. "psd1", "json".
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Determines whether this parser can parse the given file path or extension.
        /// </summary>
        /// <param name="pathOrExtension">The file path or extension to check.</param>
        /// <returns>
        /// True if the parser can parse the given path or extension; otherwise, false.
        /// </returns>
        bool CanParse(string pathOrExtension);

        /// <summary>
        /// Parses the content stream into SettingsData.
        /// </summary>
        /// <param name="content">The stream containing the settings content.</param>
        /// <param name="sourcePath">The source path of the settings file.</param>
        /// <returns>The parsed SettingsData.</returns>
        SettingsData Parse(Stream content, string sourcePath);
    }
}