using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public abstract class ConfigurableScriptRule : IScriptRule
    {
        public bool IsRuleConfigured { get; protected set; } = false;

        public void ConfigureRule()
        {
            var arguments = Helper.Instance.GetRuleArguments(this.GetName());
            //var configurableProps = GetConfigurableProperties();
            try
            {
                var properties = this.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (arguments.ContainsKey(property.Name))
                    {
                        var type = property.PropertyType;
                        var obj = arguments[property.Name];
                        property.SetValue(
                            this,
                            System.Convert.ChangeType(obj, Type.GetTypeCode(type)));
                    }
                }
            }
            catch
            {
                // return arguments with defaults
            }

            IsRuleConfigured = true;
        }

        // private GetConfigurableProperties()
        // {

        // }

        public abstract IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName);
        public abstract string GetCommonName();
        public abstract string GetDescription();
        public abstract string GetName();
        public abstract RuleSeverity GetSeverity();
        public abstract string GetSourceName();
        public abstract SourceType GetSourceType();
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConfigurablePropertyAttribute : Attribute
    {

    }
}
