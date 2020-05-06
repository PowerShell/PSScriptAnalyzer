using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Legacy
{
    internal static class DiagnosticRecordExtensions
    {
        public static ScriptDiagnostic ToScriptDiagnostic(this DiagnosticRecord diagnostic, string scriptText)
        {
            if (diagnostic.SuggestedCorrections == null
                || !diagnostic.SuggestedCorrections.Any())
            {
                return new ScriptDiagnostic(
                    diagnostic.Message,
                    diagnostic.Extent,
                    diagnostic.Severity);
            }

            var corrections = new List<Correction>();
            foreach (CorrectionExtent legacyCorrection in diagnostic.SuggestedCorrections)
            {
                corrections.Add(legacyCorrection.ToCorrection(scriptText, diagnostic.ScriptPath));
            }

            return new ScriptDiagnostic(
                diagnostic.Message,
                diagnostic.Extent,
                diagnostic.Severity,
                corrections);
        }
    }
}
