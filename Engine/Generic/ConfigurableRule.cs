// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    // This is still an experimental class. Use at your own risk!
    public abstract class ConfigurableRule : IScriptRule
    {
        /// <summary>
        /// Indicates if the rule is enabled or not.
        ///
        /// If the rule is enabled ScriptAnalyzer engine will run it
        /// otherwise it will not.
        ///
        /// Configurable rule properties should define a default value
        /// because if reading the configuration fails we use the
        /// property's default value
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool Enable { get; protected set; }

        /// <summary>
        /// Initialize the configurable properties of a configurable rule.
        /// </summary>
        protected ConfigurableRule()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// Sets the configurable properties of the rule.
        ///
        /// Properties having ConfigurableRuleProperty attribute are called configurable properties.
        /// </summary>
        /// <param name="paramValueMap">A dictionary that maps parameter name to it value. Must be non-null</param>
        public virtual void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            if (paramValueMap == null)
            {
                throw new ArgumentNullException(nameof(paramValueMap));
            }

            try
            {
                foreach (var property in GetConfigurableProperties())
                {
                    if (paramValueMap.ContainsKey(property.Name))
                    {
                        SetValue(property, paramValueMap[property.Name]);
                    }
                }
            }
            catch
            {
                // we do not know how to handle an exception in this case yet!
                // but we know that this should not crash the program
                // hence we revert the property values to their default
                SetDefaultValues();
            }
        }

        /// <summary>
        /// Analyzes the given abstract syntax tree (AST) and returns diagnostic records based on the analysis.
        /// </summary>
        /// <param name="ast">AST representing the file content</param>
        /// <param name="fileName">Path of the file corresponding to the AST</param>
        /// <returns>The results of the analysis</returns>
        public abstract IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName);

        /// <summary>
        /// Retrieves the Common name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        public abstract string GetCommonName();

        /// <summary>
        /// Retrieves the description of the rule.
        /// </summary>
        /// <returns>The description of the rule.</returns>
        public abstract string GetDescription();

        /// <summary>
        /// Retrieves the name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        public abstract string GetName();

        /// <summary>
        /// Retrieves severity of the rule.
        /// </summary>
        /// <returns>The severity of the rule.</returns>
        public abstract RuleSeverity GetSeverity();

        /// <summary>
        /// Retrieves the source name of the rule.
        /// </summary>
        /// <returns>The source name of the rule.</returns>
        public abstract string GetSourceName();

        /// <summary>
        /// Retrieves the source type of the rule.
        /// </summary>
        /// <returns>The source type of the rule.</returns>
        public abstract SourceType GetSourceType();

        private void SetDefaultValues()
        {
            foreach (var property in GetConfigurableProperties())
            {
                SetValue(property, GetDefaultValue(property));
            }
        }

        private void SetValue(PropertyInfo property, object value)
        {
            if (IsArray(property.PropertyType, out Type elementType))
            {
                object newArray = LanguagePrimitives.ConvertTo(value, elementType.MakeArrayType());
                property.SetValue(this, newArray);
                return;
            }

            // TODO Check if type is convertible
            property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
        }

        private IEnumerable<PropertyInfo> GetConfigurableProperties()
        {
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.GetCustomAttribute(typeof(ConfigurableRulePropertyAttribute)) != null)
                {
                    yield return property;
                }
            }
        }

        private Object GetDefaultValue(PropertyInfo property)
        {
            var attr = property.GetCustomAttribute(typeof(ConfigurableRulePropertyAttribute));
            if (attr == null)
            {
                throw new ArgumentException(
                    String.Format(Strings.ConfigurableScriptRulePropertyHasNotAttribute, property.Name),
                    nameof(property));
            }

            return ((ConfigurableRulePropertyAttribute)attr).DefaultValue;
        }

        private static bool IsArray(Type propertyType, out Type elementType)
        {
            if (propertyType.IsArray)
            {
                elementType = propertyType.GetElementType();
                return true;
            }

            elementType = null;
            return false;
        }
    }
}
