// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// The attribute class to designate if a property is configurable or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurableRulePropertyAttribute : Attribute
    {
        /// <summary>
        /// Default value of the property that the attribute decorates.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Initialize the attribute with the decorated property's default value.
        /// </summary>
        /// <param name="defaultValue"></param>
        public ConfigurableRulePropertyAttribute(object defaultValue)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue), Strings.ConfigurableScriptRuleNRE);
            }

            DefaultValue = defaultValue;
        }
    }
}
