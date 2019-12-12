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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseProcessBlockForPipelineCommand : IScriptRule
    {
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            IEnumerable<Ast> scriptblockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);
            
            foreach (ScriptBlockAst scriptblockAst in scriptblockAsts)
            {
                if (scriptblockAst.ProcessBlock != null
                    || scriptblockAst.ParamBlock?.Attributes == null
                    || scriptblockAst.ParamBlock.Parameters == null)
                {
                    continue;
                }
                
                foreach (ParameterAst paramAst in scriptblockAst.ParamBlock.Parameters)
                {
                    foreach (AttributeBaseAst paramAstAttribute in paramAst.Attributes)
                    {
                        if (!(paramAstAttribute is AttributeAst paramAttributeAst)) { continue; }

                        if (paramAttributeAst.NamedArguments == null) { continue; }

                        foreach (NamedAttributeArgumentAst namedArgument in paramAttributeAst.NamedArguments)
                        {
                            if (!namedArgument.ArgumentName.Equals("valuefrompipeline", StringComparison.OrdinalIgnoreCase)
                                && !namedArgument.ArgumentName.Equals("valuefrompipelinebypropertyname", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

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
