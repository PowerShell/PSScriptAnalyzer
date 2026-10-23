// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    // Owned by one AnalyzeSyntaxTree call and explicitly installed on its rule workers.
    // It cannot outlive that analysis or affect unrelated public GetModuleManifest callers.
    internal sealed class ModuleManifestAnalysisCache
    {
        [ThreadStatic]
        internal static ModuleManifestAnalysisCache Current;

        private readonly Dictionary<string, Result> results = new Dictionary<string, Result>(StringComparer.Ordinal);

        internal PSModuleInfo Get(Helper helper, string path, out IEnumerable<ErrorRecord> errors)
        {
            lock (results)
            {
                if (!results.TryGetValue(path, out var result))
                {
                    var module = helper.GetModuleManifest(path, out var moduleErrors);
                    result = new Result { Module = module, Errors = moduleErrors };
                    results.Add(path, result);
                }
                errors = result.Errors;
                return result.Module;
            }
        }

        internal Scope Enter() => new Scope(this);

        internal struct Scope : IDisposable
        {
            private readonly ModuleManifestAnalysisCache previous;

            internal Scope(ModuleManifestAnalysisCache cache)
            {
                previous = Current;
                Current = cache;
            }

            public void Dispose() => Current = previous;
        }

        private sealed class Result
        {
            internal PSModuleInfo Module;
            internal IEnumerable<ErrorRecord> Errors;
        }
    }
}
