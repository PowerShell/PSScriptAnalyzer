using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// This class extends AstVisitor2 and will skip any typedefinitionast
    /// </summary>
    public class SkipTypeDefinition : AstVisitor2
    {
        /// <summary>
        /// File name
        /// </summary>
        public string fileName;

        /// <summary>
        /// My Diagnostic Records
        /// </summary>
        public List<DiagnosticRecord> DiagnosticRecords = new List<DiagnosticRecord>();

        /// <summary>
        /// Skip typedefinition
        /// </summary>
        /// <param name="typeDefAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefAst)
        {
            return AstVisitAction.SkipChildren;
        }
    }
}
