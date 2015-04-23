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
    /// An interface for an analyzer rule that analyzes the Ast.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// GetName: Retrieves the name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        string GetName();

        /// <summary>
        /// GetName: Retrieves the Common name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        string GetCommonName();

        /// <summary>
        /// GetDescription: Retrieves the description of the rule.
        /// </summary>
        /// <returns>The description of the rule.</returns>
        string GetDescription();

        /// <summary>
        /// GetSourceName: Retrieves the source name of the rule.
        /// </summary>
        /// <returns>The source name of the rule.</returns>
        string GetSourceName();

        /// <summary>
        /// GetSourceType: Retrieves the source type of the rule.
        /// </summary>
        /// <returns>The source type of the rule.</returns>
        SourceType GetSourceType();

        /// <summary>
        /// GetSeverity: Retrieves severity of the rule.
        /// </summary>
        /// <returns></returns>
        RuleSeverity GetSeverity();

    }
}
