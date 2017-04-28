using System;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Class to provide information about an edit
    /// </summary>
    public class TextEdit
    {
        /// <summary>
        /// 1-based line number on which the text, which needs to be replaced, starts.
        /// </summary>
        public int StartLineNumber { get; }

        /// <summary>
        /// 1-based offset on start line at which the text, which needs to be replaced, starts.
        /// This includes the first character of the text.
        /// </summary>
        public int StartColumnNumber { get; }

        /// <summary>
        /// 1-based line number on which the text, which needs to be replace, ends.
        /// </summary>
        public int EndLineNumber { get; }

        /// <summary>
        /// 1-based offset on end line at which the text, which needs to be replaced, ends.
        /// This offset value is 1 more than the offset of the last character of the text.
        /// </summary>
        public int EndColumnNumber { get; }

        /// <summary>
        /// The text that will replace the text bounded by the Line/Column number properties.
        /// </summary>
        public string Text { get; }

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
        {
            StartLineNumber = startLineNumber;
            StartColumnNumber = startColumnNumber;
            EndLineNumber = endLineNumber;
            EndColumnNumber = endColumnNumber;
            Text = newText;
            ThrowIfInvalidArguments();
        }

        private void ThrowIfInvalidArguments()
        {
            ThrowIfNull<string>(Text, "text");

            // TODO Localize
            ThrowIfDecreasing(
                StartLineNumber,
                EndLineNumber,
                "start line number cannot be less than end line number");
            if (StartLineNumber == EndLineNumber)
            {
                ThrowIfDecreasing(
                    StartColumnNumber,
                    EndColumnNumber,
                    "start column number cannot be less than end column number for a one line extent");
            }
        }

        private void ThrowIfDecreasing(int start, int end, string message)
        {
            if (start > end)
            {
                throw new ArgumentException(message);
            }
        }

        private void ThrowIfNull<T>(T arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }
    }
}
