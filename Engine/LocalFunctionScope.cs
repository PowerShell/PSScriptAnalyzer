// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<string> privateInScopeNames;
        private readonly List<KeyValuePair<Ast, HashSet<string>>> childScopeNames;

        private LocalFunctionScope(
            Dictionary<string, List<FunctionDefinitionAst>> definitionsByName,
            HashSet<string> alsoInScopeNames,
            HashSet<string> privateInScopeNames,
            List<KeyValuePair<Ast, HashSet<string>>> childScopeNames)
        {
            this.definitionsByName = definitionsByName;
            this.alsoInScopeNames = alsoInScopeNames;
            this.privateInScopeNames = privateInScopeNames;
            this.childScopeNames = childScopeNames;
        }

        internal static LocalFunctionScope FromAst(Ast ast) => FromAst(ast, null, null);

        internal static LocalFunctionScope FromAst(Ast ast, IEnumerable<string> alsoInScope) => FromAst(ast, alsoInScope, null);

        // Scope containers are numbered in document order so that a position recorded against one parse
        // of a file can be found again in a later parse of the same file.
        internal static int GetScopeOrdinal(Ast root, Ast container)
        {
            int ordinal = 0;
            foreach (Ast node in root.FindAll(IsScopeCandidate, true))
            {
                if (ReferenceEquals(node, container))
                {
                    return ordinal;
                }
                ordinal++;
            }
            return -1;
        }

        private static bool IsScopeCandidate(Ast node) => node is FunctionDefinitionAst || node is ScriptBlockAst;

        /// <summary>The owning child scope of a dot-source command, or null when it runs at file scope.</summary>
        internal static Ast GetOwningScope(Ast node) => GetContainingScope(node);

        internal struct ChildScopeNames
        {
            internal int ContainerOrdinal;
            internal HashSet<string> Names;
        }

        internal static LocalFunctionScope FromAst(
            Ast ast, IEnumerable<string> alsoInScope, IEnumerable<ChildScopeNames> childScopes)
        {
            var definitionsByName = new Dictionary<string, List<FunctionDefinitionAst>>(StringComparer.OrdinalIgnoreCase);
            var childScopeNames = new List<KeyValuePair<Ast, HashSet<string>>>();
            if (ast != null && childScopes != null)
            {
                var candidates = ast.FindAll(IsScopeCandidate, true).ToList();
                foreach (var childScope in childScopes)
                {
                    if (childScope.ContainerOrdinal >= 0 && childScope.ContainerOrdinal < candidates.Count)
                    {
                        childScopeNames.Add(new KeyValuePair<Ast, HashSet<string>>(
                            candidates[childScope.ContainerOrdinal], childScope.Names));
                    }
                }
            }
            if (ast != null)
            {
                foreach (FunctionDefinitionAst function in ast.FindAll(node => node is FunctionDefinitionAst, true))
                {
                    if (string.IsNullOrWhiteSpace(function.Name))
                    {
                        continue;
                    }

                    string name = Helper.Instance.FunctionNameWithoutScope(function.Name);
                    if (!definitionsByName.TryGetValue(name, out var definitions))
                    {
                        definitions = new List<FunctionDefinitionAst>();
                        definitionsByName[name] = definitions;
                    }
                    definitions.Add(function);
                }
            }

            var alsoInScopeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var privateInScopeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (alsoInScope != null)
            {
                foreach (string name in alsoInScope)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var names = name.StartsWith("private:", StringComparison.OrdinalIgnoreCase)
                            ? privateInScopeNames : alsoInScopeNames;
                        names.Add(Helper.Instance.FunctionNameWithoutScope(name));
                    }
                }
            }
            return new LocalFunctionScope(definitionsByName, alsoInScopeNames, privateInScopeNames, childScopeNames);
        }

        private static bool IsWithin(Ast node, Ast container)
        {
            for (Ast current = node; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, container))
                {
                    return true;
                }
            }
            return false;
        }

        // A script block kept in a variable, hashtable, array or return value is not executed here.
        // Blocks passed to commands or methods are assumed to run in the enclosing scope.
        private static bool IsStoredScriptBlock(ScriptBlockExpressionAst expression)
        {
            Ast parent = expression.Parent;
            while (parent is CommandExpressionAst || parent is PipelineAst || parent is ParenExpressionAst)
            {
                parent = parent.Parent;
            }
            return parent is AssignmentStatementAst || parent is HashtableAst
                || parent is ArrayLiteralAst || parent is ArrayExpressionAst || parent is ReturnStatementAst;
        }

        /// <summary>
        /// Returns the enclosing function, directly invoked child script block or stored script block,
        /// or null for file scope. Dot-sourced blocks execute in the caller's scope rather than
        /// introducing a child scope.
        /// </summary>
        private static Ast GetContainingScope(Ast node)
        {
            for (Ast parent = node.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is FunctionDefinitionAst enclosingFunction)
                {
                    return enclosingFunction;
                }
                if (parent is ScriptBlockAst block && block.Parent is ScriptBlockExpressionAst expression)
                {
                    if (expression.Parent is CommandAst invocation && invocation.InvocationOperator == TokenKind.Ampersand
                        && ReferenceEquals(invocation.CommandElements[0], expression))
                    {
                        return block;
                    }
                    if (IsStoredScriptBlock(expression))
                    {
                        return block;
                    }
                }
            }
            return null;
        }

        private static bool HasScriptWideScope(FunctionDefinitionAst function) =>
            function.Name.StartsWith("script:", StringComparison.OrdinalIgnoreCase)
            || function.Name.StartsWith("global:", StringComparison.OrdinalIgnoreCase);

        // Dot-source groups must not flatten local/private definitions inside child scopes.
        internal static bool IsVisibleAcrossFiles(FunctionDefinitionAst function) =>
            HasScriptWideScope(function) || GetContainingScope(function) == null;

        private static bool IsVisibleTo(FunctionDefinitionAst function, Ast callSite)
        {
            if (HasScriptWideScope(function))
            {
                return true;
            }

            Ast container = GetContainingScope(function);
            if (function.Name.StartsWith("private:", StringComparison.OrdinalIgnoreCase))
            {
                return ReferenceEquals(container, GetContainingScope(callSite));
            }
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

            foreach (var childScope in childScopeNames)
            {
                if (childScope.Value.Contains(commandName) && (callSite == null || IsWithin(callSite, childScope.Key)))
                {
                    return true;
                }
            }

            if (privateInScopeNames.Contains(commandName)
                && (callSite == null || GetContainingScope(callSite) == null))
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
