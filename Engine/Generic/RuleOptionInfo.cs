// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{

    /// <summary>
    /// Holds metadata for a single configurable rule property.
    /// </summary>
    public class RuleOptionInfo
    {
        public string Name { get; internal set; }
        public Type OptionType { get; internal set; }
        public object DefaultValue { get; internal set; }
    }

}
