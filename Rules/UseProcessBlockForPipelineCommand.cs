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
    public class UseProcessBlockForPipelineCommand : IScriptRule
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
                        if (!(paramAstAttribute is AttributeAst)) { continue; }

                        var namedArguments = (paramAstAttribute as AttributeAst).NamedArguments;
                        if (namedArguments == null) { continue; }

                        foreach (NamedAttributeArgumentAst namedArgument in namedArguments)
                        {
                            if
                            (
                                !namedArgument.ArgumentName.Equals("valuefrompipeline", StringComparison.OrdinalIgnoreCase)
                                && !namedArgument.ArgumentName.Equals("valuefrompipelinebypropertyname", StringComparison.OrdinalIgnoreCase)
                            )
                            { continue; }

                            yield return new DiagnosticRecord(
                                string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandError, paramAst.Name.VariablePath.UserPath),
                                paramAst.Name.Extent,
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName,
                                paramAst.Name.VariablePath.UserPath
                            );
                        }
                    }
                }
            }
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseProcessBlockForPipelineCommandName);
        }

        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseProcessBlockForPipelineCommandDescription);
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
