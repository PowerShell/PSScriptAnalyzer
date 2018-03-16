// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// This class extends AstVisitor and will skip any typedefinitionast
    /// </summary>
    public class SkipTypeDefinition : AstVisitor
    {
        /// <summary>
        /// File name
        /// </summary>
        public string fileName;

        /// <summary>
        /// My Diagnostic Records
        /// </summary>
        public List<DiagnosticRecord> DiagnosticRecords = new List<DiagnosticRecord>();

    }
}
