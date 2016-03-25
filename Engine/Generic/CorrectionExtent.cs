using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public class CorrectionExtent
    {
        public int EndColumnNumber
        {
            get
            {
                return endColumnNumber;
            }
        }

        public int EndLineNumber
        {
            get
            {
                return endLineNumber;
            }
        }

        public string File
        {
            get
            {
                return file;
            }
        }

        public int StartColumnNumber
        {
            get
            {
                return startColumnNumber;
            }
        }

        public int StartLineNumber
        {
            get
            {
                return startLineNumber;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        private string file;
        private int startLineNumber;
        private int endLineNumber;
        private int startColumnNumber;
        private int endColumnNumber;
        private string text;
    
        public CorrectionExtent(int startLineNumber, int endLineNumber, int startColumnNumber, int endColumnNumber, string text, string file)
        {
            this.startLineNumber = startLineNumber;
            this.endLineNumber = endLineNumber;
            this.startColumnNumber = startColumnNumber;
            this.endColumnNumber = endColumnNumber;
            this.file = file;
            this.text = text;
            ThrowIfInvalidArguments();
        }

        private void ThrowIfInvalidArguments()
        {
            ThrowIfNull<string>(file, "filename");
            ThrowIfNull<string>(text, "text");
            ThrowIfDecreasing(startLineNumber, endLineNumber, "start line number cannot be less than end line number");            
            if (startLineNumber == endLineNumber)
            {
                ThrowIfDecreasing(StartColumnNumber, endColumnNumber, "start column number cannot be less than end column number for a one line extent");
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
