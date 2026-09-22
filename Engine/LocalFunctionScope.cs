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

        private readonly Dictionary<string, List<FunctionDefinitionAst>> definitionsByName;
        private readonly HashSet<string> alsoInScopeNames;

        private LocalFunctionScope(
            Dictionary<string, List<FunctionDefinitionAst>> definitionsByName,
            HashSet<string> alsoInScopeNames)
        {
            this.definitionsByName = definitionsByName;
            this.alsoInScopeNames = alsoInScopeNames;
        }

        internal static LocalFunctionScope FromAst(Ast ast) => FromAst(ast, null);

        internal static LocalFunctionScope FromAst(Ast ast, IEnumerable<string> alsoInScope)
        {
            var definitionsByName = new Dictionary<string, List<FunctionDefinitionAst>>(StringComparer.OrdinalIgnoreCase);
            if (ast != null)
            {
                foreach (FunctionDefinitionAst function in ast.FindAll(node => node is FunctionDefinitionAst, true))
                {
                    if (string.IsNullOrWhiteSpace(function.Name))
                    {
                        continue;
                    }

                    if (!definitionsByName.TryGetValue(function.Name, out var definitions))
                    {
                        definitions = new List<FunctionDefinitionAst>();
                        definitionsByName[function.Name] = definitions;
                    }
                    definitions.Add(function);
                }
            }

            var alsoInScopeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (alsoInScope != null)
            {
                alsoInScopeNames.UnionWith(alsoInScope);
            }
            return new LocalFunctionScope(definitionsByName, alsoInScopeNames);
        }

        /// <summary>
        /// Returns the containing function of <paramref name="function"/>, or null if it is defined at the
        /// top level of the file. A function nested inside another is only in scope for calls made from
        /// within that containing function's body, not for the whole file.
        /// </summary>
        private static Ast GetContainingFunction(FunctionDefinitionAst function)
        {
            for (Ast parent = function.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is FunctionDefinitionAst enclosingFunction)
                {
                    return enclosingFunction;
                }
            }
            return null;
        }

        private static bool IsVisibleTo(FunctionDefinitionAst function, Ast callSite)
        {
            Ast container = GetContainingFunction(function);
            if (container == null)
            {
                // Defined at the top level of the file: visible everywhere in it.
                return true;
            }

            IScriptExtent containerExtent = container.Extent;
            IScriptExtent callSiteExtent = callSite.Extent;
            return callSiteExtent.StartOffset >= containerExtent.StartOffset
                && callSiteExtent.EndOffset <= containerExtent.EndOffset;
        }

        /// <param name="callSite">
        /// The Ast of the command being resolved. When omitted, any matching definition anywhere in the
        /// file is conservatively treated as in scope, matching this method's previous, file-wide behavior.
        /// </param>
        internal bool IsDefinedInScript(string commandName, Ast callSite = null)
        {
            if (commandName == null)
            {
                return false;
            }

            if (alsoInScopeNames.Contains(commandName))
            {
                return true;
            }

            if (!definitionsByName.TryGetValue(commandName, out var definitions))
            {
                return false;
            }

            if (callSite == null)
            {
                return true;
            }

            foreach (var definition in definitions)
            {
                if (IsVisibleTo(definition, callSite))
                {
                    return true;
                }
            }
            return false;
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
