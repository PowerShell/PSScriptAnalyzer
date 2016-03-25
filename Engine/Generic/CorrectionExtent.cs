using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public class CorrectionExtent : IScriptExtent
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

        public int EndOffset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IScriptPosition EndScriptPosition
        {
            get
            {
                throw new NotImplementedException();
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

        public int StartOffset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IScriptPosition StartScriptPosition
        {
            get
            {
                throw new NotImplementedException();
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
    
        public CorrectionExtent(string file, int startLineNumber, int endLineNumber, int startColumnNumber, int endColumnNumber, string text)
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
            ThrowIfTextNotConsistent();
        }

        private void ThrowIfTextNotConsistent()
        {
            using (var stringReader = new StringReader(text))
            {
                int numLines = 0;                
                int expectedNumLines = endLineNumber - startLineNumber + 1;                
                for (string line = stringReader.ReadLine(); line != null; line = stringReader.ReadLine())
                {
                    numLines++;                    
                }
                if (numLines != expectedNumLines)
                {
                    throw new ArgumentException("number of lines not consistent with text argument");
                }
                if (numLines == 1)
                {
                    ThrowIfDecreasing(startColumnNumber, endColumnNumber, "start column number cannot be less then end column number");
                    if (endColumnNumber - startColumnNumber != text.Length)
                    {
                        throw new ArgumentException("column numbers are inconsistent with the length of the string");
                    }
                }
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
