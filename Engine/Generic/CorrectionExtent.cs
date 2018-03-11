//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public class CorrectionExtent : TextEdit
    {
        public string File
        {
            get
            {
                return file;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

       private string file;
       private string description;

        public CorrectionExtent(
            int startLineNumber,
            int endLineNumber,
            int startColumnNumber,
            int endColumnNumber,
            string text,
            string file)
            : this(
                  startLineNumber,
                  endLineNumber,
                  startColumnNumber,
                  endColumnNumber,
                  text,
                  file,
                  null)
        {
        }

        public CorrectionExtent(
            int startLineNumber,
            int endLineNumber,
            int startColumnNumber,
            int endColumnNumber,
            IEnumerable<string> lines,
            string file,
            string description)
            : base(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber, lines)
        {
            this.file = file;
            this.description = description;
        }

        public CorrectionExtent(
            int startLineNumber,
            int endLineNumber,
            int startColumnNumber,
            int endColumnNumber,
            string text,
            string file,
            string description)
            : base(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber, text)
        {
            this.file = file;
            this.description = description;
        }

        public CorrectionExtent(
            IScriptExtent violationExtent,
            string replacementText,
            string filePath,
            string description)
            : this(
                violationExtent.StartLineNumber,
                violationExtent.EndLineNumber,
                violationExtent.StartColumnNumber,
                violationExtent.EndColumnNumber,
                replacementText,
                filePath,
                description)
        {

        }

        public CorrectionExtent(
            IScriptExtent violationExtent,
            string replacementText,
            string filePath)
            : this(
                violationExtent.StartLineNumber,
                violationExtent.EndLineNumber,
                violationExtent.StartColumnNumber,
                violationExtent.EndColumnNumber,
                replacementText,
                filePath)
        {

        }
    }
}
