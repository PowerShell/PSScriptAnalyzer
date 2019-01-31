using System.Collections.Generic;
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public class UseCompatibleTypes : CompatibilityRule
    {
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesCommonName);
        }

        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesDescription);
        }

        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleTypesName);
        }

        protected override CompatibilityVisitor CreateVisitor(string fileName)
        {
            return new TypeCompatibilityVisitor();
        }

        private class TypeCompatibilityVisitor : CompatibilityVisitor
        {
            public override IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return new CompatibilityDiagnostic[0];
            }
        }
    }
}