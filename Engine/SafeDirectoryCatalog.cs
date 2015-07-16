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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides a simple DirectoryCatalog implementation that doesn't
    /// fail assembly loading when it encounters a ReflectionTypeLoadException.
    /// The default DirectoryCatalog implementation stops evaluating any
    /// remaining assemblies in the directory if one of these exceptions
    /// are encountered.
    /// </summary>
    internal class SafeDirectoryCatalog : AggregateCatalog
    {
        public SafeDirectoryCatalog(string folderLocation, IOutputWriter outputWriter)
        {
            if (outputWriter == null)
            {
                throw new ArgumentNullException("outputWriter");
            }

            // Make sure the directory actually exists
            var directoryInfo = new DirectoryInfo(folderLocation);
            if (directoryInfo.Exists == false)
            {
                throw new CompositionException(
                    "The specified folder does not exist: " + directoryInfo.FullName);
            }

            // Load each DLL found in the directory
            foreach (var dllFile in directoryInfo.GetFileSystemInfos("*.dll"))
            {
                try
                {
                    // Attempt to create an AssemblyCatalog for this DLL
                    var assemblyCatalog =
                        new AssemblyCatalog(
                            Assembly.LoadFile(
                                dllFile.FullName));

                    // We must call ToArray here to pre-initialize the Parts
                    // IEnumerable and cause it to be stored.  The result is
                    // not used here because it will be accessed later once
                    // the composition container starts assembling parts.
                    assemblyCatalog.Parts.ToArray();

                    this.Catalogs.Add(assemblyCatalog);
                }
                catch (ReflectionTypeLoadException e)
                {
                    // Write out the exception details and allow the
                    // loading process to continue
                    outputWriter.WriteWarning(e.ToString());
                }
            }
        }
    }
}
