using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    // This is still an experimental class. Use at your own risk!
    public abstract class ConfigurableScriptRule : IScriptRule
    {
        // Configurable rules should define a default value
        // because if reading the configuration fails
        // we use the property's default value
        [ConfigurableRuleProperty()]
        public bool Enable { get; protected set; } = false;

        public virtual void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            if (paramValueMap == null)
            {
                throw new ArgumentNullException(nameof(paramValueMap));
            }

            try
            {
                var properties = GetConfigurableProperties();
                foreach (var property in properties)
                {
                    if (paramValueMap.ContainsKey(property.Name))
                    {
                        var type = property.PropertyType;
                        var obj = paramValueMap[property.Name];

                        // TODO Check if type is convertible
                        property.SetValue(
                            this,
                            System.Convert.ChangeType(obj, type));
                    }
                }
            }
            catch
            {
                // we do not know how to handle an exception in this case yet!
                // but we know that this should not crash the program hence we
                // have this empty catch block
            }
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

    }
}
