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
    /// <summary>
    /// Represents an interface for a DSC rule that analyzes a DSC resource
    /// </summary>
    public interface IDSCResourceRule : IRule
    {
        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName);

#if !PSV3
        /// <summary>
        /// Analyze dsc classes (if any) in the file
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName);
#endif
    }
}