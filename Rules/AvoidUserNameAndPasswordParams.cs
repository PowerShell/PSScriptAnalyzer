// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsernameAndPasswordParams: Check that a function does not use both username and password
    /// parameters.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsernameAndPasswordParams : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check that a function does not use both username
        /// and password parameters.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            List<String> passwords = new List<String>() {"Password", "Passphrase"};
            List<String> usernames = new List<String>() { "Username", "User"};
            Type[] typeWhiteList = {typeof(CredentialAttribute),
                                            typeof(PSCredential),
                                            typeof(System.Security.SecureString),
                                            typeof(SwitchParameter),
                                            typeof(Boolean)};

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                bool hasPwd = false;
                bool hasUserName = false;

                // Finds all ParamAsts.
                IEnumerable<Ast> paramAsts = funcAst.FindAll(testAst => testAst is ParameterAst, true);
                ParameterAst usernameAst = null;
                ParameterAst passwordAst = null;
                // Iterates all ParamAsts and check if their names are on the list.
                foreach (ParameterAst paramAst in paramAsts)
                {
                    var attributes = typeWhiteList.Select(x => GetAttributeOfType(paramAst.Attributes, x));
                    String paramName = paramAst.Name.VariablePath.ToString();                    
                    foreach (String password in passwords)
                    {
                        if (paramName.IndexOf(password, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            if (attributes.Any(x => x != null))
                            {
                                continue;
                            }

                            hasPwd = true;
                            passwordAst = paramAst;
                            break;
                        }
                    }

                    foreach (String username in usernames)
                    {
                        if (paramName.IndexOf(username, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            hasUserName = true;
                            usernameAst = paramAst;
                            break;
                        }
                    }
                }

                if (hasUserName && hasPwd)
                {
                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsError, funcAst.Name),
                        GetExtent(usernameAst, passwordAst, ast), GetName(), DiagnosticSeverity.Error, fileName);
                }
            }
        }

        private AttributeBaseAst GetAttributeOfType(IEnumerable<AttributeBaseAst> attributeAsts, Type type)
        {
            return attributeAsts.FirstOrDefault(x => IsAttributeOfType(x, type));
        }

        private bool IsAttributeOfType(AttributeBaseAst attributeAst, Type type)
        {
            var arrayType = attributeAst.TypeName as ArrayTypeName;
            if (arrayType != null)
            {
                return arrayType.ElementType.GetReflectionType() == type;
            }
            return attributeAst.TypeName.GetReflectionType() == type;
        }
        /// <summary>
        /// Returns script extent of username and password parameters
        /// </summary>
        /// <param name="usernameAst"></param>
        /// <param name="passwordAst"></param>
        /// <returns>IScriptExtent</returns>
        private IScriptExtent GetExtent(ParameterAst usernameAst, ParameterAst passwordAst, Ast scriptAst)
        {
            var usrExt = usernameAst.Extent;
            var pwdExt = passwordAst.Extent;
            IScriptExtent startExt, endExt;
            var usrBeforePwd 
                = (usrExt.StartLineNumber == pwdExt.StartLineNumber
                    && usrExt.StartColumnNumber < pwdExt.StartColumnNumber)
                    || usrExt.StartLineNumber < pwdExt.StartLineNumber;
            if (usrBeforePwd)
            {
                startExt = usrExt;
                endExt = pwdExt;
            }
            else
            {
                startExt = pwdExt;
                endExt = usrExt;
            }
            var startPos = new ScriptPosition(
                startExt.File,
                startExt.StartLineNumber,
                startExt.StartColumnNumber,
                startExt.StartScriptPosition.Line);
            var endPos = new ScriptPosition(
                endExt.File,
                endExt.EndLineNumber,
                endExt.EndColumnNumber,
                endExt.EndScriptPosition.Line);
            return new ScriptExtent(startPos, endPos);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsernameAndPasswordParamsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
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
            return RuleSeverity.Error;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
