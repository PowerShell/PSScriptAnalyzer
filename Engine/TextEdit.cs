// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Class to provide information about an edit
    /// </summary>
    public class TextEdit : Range
    {
        /// <summary>
        /// 1-based line number on which the text, which needs to be replaced, starts.
        /// </summary>
        public int StartLineNumber { get { return this.Start.Line; } }

        /// <summary>
        /// 1-based offset on start line at which the text, which needs to be replaced, starts.
        /// This includes the first character of the text.
        /// </summary>
        public int StartColumnNumber { get { return this.Start.Column; } }

        /// <summary>
        /// 1-based line number on which the text, which needs to be replace, ends.
        /// </summary>
        public int EndLineNumber { get { return this.End.Line; } }

        /// <summary>
        /// 1-based offset on end line at which the text, which needs to be replaced, ends.
        /// This offset value is 1 more than the offset of the last character of the text.
        /// </summary>
        public int EndColumnNumber { get { return this.End.Column; } }

        /// <summary>
        /// The text that will replace the text bounded by the Line/Column number properties.
        /// </summary>
        public string Text { get; }

        public string[] Lines { get; }
        /// <summary>
        /// Constructs a TextEdit object.
        /// </summary>
        /// <param name="startLineNumber">1-based line number on which the text, which needs to be replaced, starts. </param>
        /// <param name="startColumnNumber">1-based offset on start line at which the text, which needs to be replaced, starts. This includes the first character of the text. </param>
        /// <param name="endLineNumber">1-based line number on which the text, which needs to be replace, ends. </param>
        /// <param name="endColumnNumber">1-based offset on end line at which the text, which needs to be replaced, ends. This offset value is 1 more than the offset of the last character of the text. </param>
        /// <param name="newText">The text that will replace the text bounded by the Line/Column number properties. </param>
        public TextEdit(
            int startLineNumber,
            int startColumnNumber,
            int endLineNumber,
            int endColumnNumber,
            string newText)
            : base(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber)
        {
            if (newText == null)
            {
                throw new ArgumentNullException(nameof(newText));
            }

            Text = newText;
            Lines = Text.GetLines().ToArray();
        }

        /// <summary>
        /// Constructs a TextEdit object.
        /// </summary>
        /// <param name="startLineNumber">1-based line number on which the text, which needs to be replaced, starts. </param>
        /// <param name="startColumnNumber">1-based offset on start line at which the text, which needs to be replaced, starts. This includes the first character of the text. </param>
        /// <param name="endLineNumber">1-based line number on which the text, which needs to be replace, ends. </param>
        /// <param name="endColumnNumber">1-based offset on end line at which the text, which needs to be replaced, ends. This offset value is 1 more than the offset of the last character of the text. </param>
        /// <param name="lines">The contiguous lines that will replace the text bounded by the Line/Column number properties. </param>
        public TextEdit(
            int startLineNumber,
            int startColumnNumber,
            int endLineNumber,
            int endColumnNumber,
            IEnumerable<String> lines)
            : base(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (lines.Any(line => line == null))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture,
                    Strings.TextEditNoNullItem), nameof(lines));
            }

            Lines = lines.ToArray();
            Text = String.Join(Environment.NewLine, Lines);
        }
    }
}
