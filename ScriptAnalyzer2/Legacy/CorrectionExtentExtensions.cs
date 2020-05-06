using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Legacy
{
    internal static class CorrectionExtentExtensions
    {
        public static Correction ToCorrection(this CorrectionExtent correctionExtent, string fullScriptText, string scriptPath)
        {
            return new Correction(
                ScriptExtent.FromPositions(
                    fullScriptText,
                    scriptPath,
                    correctionExtent.StartLineNumber,
                    correctionExtent.StartColumnNumber,
                    correctionExtent.EndLineNumber,
                    correctionExtent.EndColumnNumber),
                correctionExtent.Text,
                correctionExtent.Description);
        }
    }
}
