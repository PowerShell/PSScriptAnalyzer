// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Parses PowerShell data files (.psd1) containing a top-level hashtable into
    /// <see cref="SettingsData"/>.
    /// Parsing steps:
    /// 1. Verify the source file exists (the PowerShell parser requires a path).
    /// 2. Parse the file into an AST using <see cref="Parser.ParseFile"/>.
    /// 3. Locate the first <see cref="HashtableAst"/> (expected to represent the settings).
    /// 4. Safely convert the hashtable AST into a <see cref="Hashtable"/> via
    ///    <see cref="Helper.GetSafeValueFromHashtableAst"/>.
    /// 5. Delegate normalization and validation to
    ///    <see cref="HashtableSettingsConverter.Convert"/>.
    /// Throws <see cref="InvalidDataException"/> for structural issues (missing hashtable, invalid
    /// values).
    /// </summary>
    internal sealed class Psd1SettingsParser : ISettingsParser
    {
        public string FormatName => "psd1";

        /// <summary>
        /// Determines whether the supplied path (or extension) is a .psd1 settings file.
        /// </summary>
        /// <param name="pathOrExtension">Full path or just an extension string.</param>
        /// <returns>True if the extension is .psd1 (case-insensitive).</returns>
        public bool CanParse(string pathOrExtension) =>
            string.Equals(Path.GetExtension(pathOrExtension), ".psd1", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Parses a .psd1 settings file into <see cref="SettingsData"/>.
        /// </summary>
        /// <param name="content">
        /// Stream for API symmetry; not directly consumed (PowerShell parser reads from file path).
        /// </param>
        /// <param name="sourcePath">Absolute or relative path to the .psd1 file.</param>
        /// <returns>Normalized <see cref="SettingsData"/> instance.</returns>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="InvalidDataException">
        /// If no top-level hashtable is found or conversion yields invalid data.
        /// </exception>
        public SettingsData Parse(Stream content, string sourcePath)
        {
            // Need file path for PowerShell Parser.ParseFile
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Settings file not found.", sourcePath);
            }

            Ast ast = Parser.ParseFile(sourcePath, out Token[] tokens, out ParseError[] errors);

            if (ast.FindAll(a => a is HashtableAst, false).FirstOrDefault() is not HashtableAst hashTableAst)
            {
                throw new InvalidDataException($"Settings file '{sourcePath}' does not contain a hashtable.");
            }

            Hashtable raw;
            try
            {
                raw = Helper.GetSafeValueFromHashtableAst(hashTableAst);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidDataException($"Invalid settings file '{sourcePath}'.", e);
            }
            if (raw == null)
            {
                throw new InvalidDataException($"Invalid settings file '{sourcePath}'.");
            }

            return HashtableSettingsConverter.Convert(raw);
        }
    }

}