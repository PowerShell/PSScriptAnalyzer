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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for an analyzer rule that analyzes the Ast.
    /// </summary>
    public interface IScriptRule : IRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the given Ast and returns DiagnosticRecords based on the anaylsis.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName);
    }
}
