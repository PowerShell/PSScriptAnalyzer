// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Interface for settings file formats
    /// </summary>
    public interface ISettingsFormat
    {
        /// <summary>
        /// Format identifier (extension without dot, e.g. 'json', 'psd1').
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Whether this format can handle the specified path
        /// </summary>
        /// <param name="path">Full path or filename.</param>
        /// <returns>True if this format can handle the path.</returns>
        bool Supports(string path);

        /// <summary>
        /// Deserialises the content stream into <see cref="SettingsData"/>.
        /// </summary>
        /// <param name="content">The content to parse.</param>
        /// <returns>The parsed SettingsData.</returns>
        SettingsData Deserialize(string content, string sourcePath);

        /// <summary>
        /// Serializes the <see cref="SettingsData"/> into a string representation.
        /// </summary>
        /// <param name="settingsData"></param>
        /// <returns></returns>
        string Serialize(SettingsData settingsData);

    }
}