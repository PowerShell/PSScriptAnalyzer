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
        // TODO: Remove this parameter or make it optional.
        //       Properties already have a way to specify default values in C#.
        //       Having this prevents using null (which may be legitimate)
        //       or values from other assemblies, and overrides the constructor,
        //       which just makes life harder.

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
            // TODO: null is a legitimate value and should be allowed.
            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue), Strings.ConfigurableScriptRuleNRE);
            }

            DefaultValue = defaultValue;
        }
    }
}
