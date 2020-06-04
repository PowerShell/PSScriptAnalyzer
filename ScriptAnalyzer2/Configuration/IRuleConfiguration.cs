using Microsoft.PowerShell.ScriptAnalyzer.Builtin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration
{
    public class CommonConfiguration
    {
        public static CommonConfiguration Default = new CommonConfiguration(enabled: true);

        [JsonConstructor]
        public CommonConfiguration(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; } = true;
    }

    public interface IRuleConfiguration
    {
        CommonConfiguration Common { get; }

        IRuleConfiguration AsTypedConfiguration(Type configurationType);
    }

    public class RuleConfiguration : IRuleConfiguration
    {
        public static RuleConfiguration Default { get; } = new RuleConfiguration(CommonConfiguration.Default);

        public RuleConfiguration(CommonConfiguration common)
        {
            Common = common;
        }

        public CommonConfiguration Common { get; }

        public virtual IRuleConfiguration AsTypedConfiguration(Type configurationType)
        {
            if (configurationType == typeof(RuleConfiguration))
            {
                return this;
            }

            return null;
        }
    }

    public abstract class LazyConvertedRuleConfiguration<TConfiguration> : IRuleConfiguration
    {
        private readonly TConfiguration _configurationObject;

        private IRuleConfiguration _convertedObject;

        private Type _convertedObjectType;

        protected LazyConvertedRuleConfiguration(
            CommonConfiguration commonConfiguration,
            TConfiguration configurationObject)
        {
            _configurationObject = configurationObject;
            Common = commonConfiguration;
        }

        public CommonConfiguration Common { get; }

        public abstract bool TryConvertObject(Type type, TConfiguration configuration, out IRuleConfiguration result);

        public IRuleConfiguration AsTypedConfiguration(Type configurationType)
        {
            if (_convertedObject != null
                && configurationType.IsAssignableFrom(_convertedObjectType))
            {
                return _convertedObject;
            }

            if (TryConvertObject(configurationType, _configurationObject, out _convertedObject))
            {
                _convertedObjectType = configurationType;
                return _convertedObject;
            }

            return null;
        }
    }
}
