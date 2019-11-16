using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseProcessBlockForPipelineCommands : IScriptRule
    {
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                if
                (
                    funcAst.Body.ParamBlock == null
                    || funcAst.Body.ParamBlock.Attributes == null
                    || funcAst.Body.ParamBlock.Parameters == null
                    || funcAst.Body.ProcessBlock != null
                )
                { continue; }

                foreach (var paramAst in funcAst.Body.ParamBlock.Parameters)
                {
                    foreach (var paramAstAttribute in paramAst.Attributes)
                    {
                        foreach (NamedAttributeArgumentAst namedArgument in (paramAstAttribute as AttributeAst).NamedArguments)
                        {
                            if
                            (
                                !String.Equals(namedArgument.ArgumentName, "valuefrompipeline", StringComparison.OrdinalIgnoreCase)
                                && !String.Equals(namedArgument.ArgumentName, "valuefrompipelinebypropertyname", StringComparison.OrdinalIgnoreCase)
                            )
                            { continue; }
                        }
                    }

                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandsError, paramAst.Name.VariablePath.UserPath),
                        paramAst.Name.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        paramAst.Name.VariablePath.UserPath
                    );
                }
            }
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseProcessBlockForPipelineCommandsName);
        }

        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandsCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandsDescription);
        }

        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
