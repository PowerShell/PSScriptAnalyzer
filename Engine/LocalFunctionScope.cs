// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    // Owned by one AnalyzeSyntaxTree call and explicitly installed on its rule workers.
    // Command lookups resolve against a pristine runspace, so a name the script defines itself would
    // otherwise be answered by whatever module happens to be installed on the analyzing machine
    // rather than by the definition PowerShell would actually bind.
    internal sealed class LocalFunctionScope
    {
        [ThreadStatic]
        internal static LocalFunctionScope Current;

        private readonly HashSet<string> names;

        private LocalFunctionScope(HashSet<string> names) => this.names = names;

        internal static LocalFunctionScope FromAst(Ast ast) => FromAst(ast, null);

        internal static LocalFunctionScope FromAst(Ast ast, IEnumerable<string> alsoInScope)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (ast != null)
            {
                foreach (FunctionDefinitionAst function in ast.FindAll(node => node is FunctionDefinitionAst, true))
                {
                    if (!string.IsNullOrWhiteSpace(function.Name))
                    {
                        names.Add(function.Name);
                    }
                }
            }
            if (alsoInScope != null)
            {
                names.UnionWith(alsoInScope);
            }
            return new LocalFunctionScope(names);
        }

        internal bool IsDefinedInScript(string commandName)
        {
            return commandName != null && names.Contains(commandName);
        }

        internal Scope Enter() => new Scope(this);

        internal struct Scope : IDisposable
        {
            private readonly LocalFunctionScope previous;

            internal Scope(LocalFunctionScope scope)
            {
                previous = Current;
                Current = scope;
            }

            public void Dispose() => Current = previous;
        }
    }
}
