// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

    internal struct CorrectionComparer : IComparer<CorrectionExtent>, IEqualityComparer<CorrectionExtent>
    {
        public int Compare(CorrectionExtent x, CorrectionExtent y)
        {
            if (x.StartLineNumber > y.StartLineNumber)
            {
                return 1;
            }

            if (x.StartLineNumber < y.StartLineNumber)
            {
                return -1;
            }

            if (x.StartColumnNumber > y.StartColumnNumber)
            {
                return 1;
            }

            if (x.StartColumnNumber < y.StartColumnNumber)
            {
                return -1;
            }

            return 0;
        }

        public bool Equals(CorrectionExtent x, CorrectionExtent y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(CorrectionExtent obj)
        {
            return obj != null
                ? obj.GetHashCode()
                : 0;
        }
    }
}
