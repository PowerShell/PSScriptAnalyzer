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

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents the severity of a PSScriptAnalyzer rule
    /// </summary>
    public enum RuleSeverity : uint
    {
        /// <summary>
        /// Information: This warning is trivial, but may be useful. They are recommended by PowerShell best practice.
        /// </summary>
        Information = 0,

        /// <summary>
        /// WARNING: This warning may cause a problem or does not follow PowerShell's recommended guidelines.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// ERROR: This warning is likely to cause a problem or does not follow PowerShell's required guidelines.
        /// </summary>
        Error = 2,
    };
}
