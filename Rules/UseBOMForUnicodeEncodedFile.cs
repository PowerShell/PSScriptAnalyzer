// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseBOMForUnicodeEncodedFile: Checks if a file with missing BOM is ASCII encoded.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseBOMForUnicodeEncodedFile : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: For a file that has BOM missing, check if content is encoded in ASCII
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            // we are given a script definition, do not analyze
            if (String.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            byte[] byteStream = File.ReadAllBytes(fileName);

            if (null == GetByteStreamEncoding(byteStream))
            {
                // Did not detect the presence of BOM
                // Make sure there is no byte > 127 (0x7F) to ensure file is ASCII encoded
                // Else emit rule violation
                                
                if (0 != byteStream.Count(o => o > 0x7F))
                { 
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseBOMForUnicodeEncodedFileError, System.IO.Path.GetFileName(fileName), null),
                                null, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        /// <summary>
        /// GetByteStreamEncoding: Detect the file encoding using the file's byte stream
        /// </summary>
        private Encoding GetByteStreamEncoding(byte[] byteStream)
        {
            // Analyze BOM
            if (byteStream.Length >= 4 && byteStream[0] == 0x00 && byteStream[1] == 0x00 && byteStream[2] == 0xFE && byteStream[3] == 0xFF)
            {
                // UTF-32, big-endian 
                return Encoding.GetEncoding("utf-32BE");
            }
            else if (byteStream.Length >= 4 && byteStream[0] == 0xFF && byteStream[1] == 0xFE && byteStream[2] == 0x00 && byteStream[3] == 0x00)
            {
                // UTF-32, little-endian
                return Encoding.UTF32;
            }
            else if (byteStream.Length >= 2 && byteStream[0] == 0xFE && byteStream[1] == 0xFF)
            {
                // UTF-16, big-endian
                return Encoding.BigEndianUnicode;
            }
            else if (byteStream.Length >= 2 && byteStream[0] == 0xFF && byteStream[1] == 0xFE)
            {
                // UTF-16, little-endian
                return Encoding.Unicode;
            }
            else if (byteStream.Length >= 3 && byteStream[0] == 0xEF && byteStream[1] == 0xBB && byteStream[2] == 0xBF)
            {
                // UTF-8
                return Encoding.UTF8;
            }
            else if (byteStream.Length >= 3 && byteStream[0] == 0x2b && byteStream[1] == 0x2f && byteStream[2] == 0x76)
            {
                // UTF7
                return Encoding.UTF7;
            }

            // Did not detect BOM OR Unknown File encoding
            return null;
            
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseBOMForUnicodeEncodedFileName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseBOMForUnicodeEncodedFileCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseBOMForUnicodeEncodedFileDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




