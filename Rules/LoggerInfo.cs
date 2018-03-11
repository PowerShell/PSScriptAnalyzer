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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an internal class to properly display the name and description of a logger.
    /// </summary>
    internal class LoggerInfo
    {
        private string name;
        private string description;

        /// <summary>
        /// Name: The name of the logger.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        /// <summary>
        /// Description: The description of the logger.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Description
        {
            get { return description; }
            private set { description = value; }
        }

        /// <summary>
        /// Constructor for a LoggerInfo.
        /// </summary>
        /// <param name="name">The name of the logger</param>
        /// <param name="description">The description of the logger</param>
        public LoggerInfo(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}




