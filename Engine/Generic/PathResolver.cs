// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    internal static class PathResolver
    {
        /// <summary>
        /// A shim around the GetResolvedProviderPathFromPSPath method from PSCmdlet to resolve relative path including wildcard support.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="ProviderInfo"></typeparam>
        /// <typeparam name="GetResolvedProviderPathFromPSPathDelegate"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        internal delegate GetResolvedProviderPathFromPSPathDelegate GetResolvedProviderPathFromPSPath<in @string, ProviderInfo, out GetResolvedProviderPathFromPSPathDelegate>(@string input, out ProviderInfo output);
    }
}
