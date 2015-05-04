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
    /// Represents a source name of a script analyzer rule.
    /// </summary>
    public enum SourceType : uint
    {
        /// <summary>
        /// BUILTIN: Indicates the script analyzer rule is contributed as a built-in rule.
        /// </summary>
        Builtin = 0,

        /// <summary>
        /// MANAGED: Indicates the script analyzer rule is contirbuted as a managed rule.
        /// </summary>
        Managed = 1,

        /// <summary>
        /// MODULE: Indicates the script analyzer rule is contributed as a Windows PowerShell module rule.
        /// </summary>
        Module  = 2,
    };
}
