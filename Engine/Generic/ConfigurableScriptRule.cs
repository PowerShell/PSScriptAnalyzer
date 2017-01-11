using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    // This is still an experimental class. Use at your own risk!
    public abstract class ConfigurableScriptRule : IScriptRule
    {
        // Configurable rule properties should define a default value
        // because if reading the configuration fails
        // we use the property's default value
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool Enable { get; protected set; }

        protected ConfigurableScriptRule()
        {
            SetDefaultValues();
        }

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

        private void SetDefaultValues()
        {
            foreach (var property in GetConfigurableProperties())
            {
                SetValue(property, GetDefaultValue(property));
            }
        }

        private void SetValue(PropertyInfo property, object value)
        {
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

        public abstract IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName);
        public abstract string GetCommonName();
        public abstract string GetDescription();
        public abstract string GetName();
        public abstract RuleSeverity GetSeverity();
        public abstract string GetSourceName();
        public abstract SourceType GetSourceType();
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConfigurableRulePropertyAttribute : Attribute
    {
        public object DefaultValue { get; private set; }

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
