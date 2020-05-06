using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builder
{
    public interface IRuleComponentProvider
    {
        bool TryGetComponentInstance(Type componentType, out object component);
    }

    internal class SimpleRuleComponentProvider : IRuleComponentProvider
    {
        private readonly IReadOnlyDictionary<Type, Func<object>> _componentRegistrations;

        private readonly IReadOnlyDictionary<Type, object> _singletonComponents;

        public SimpleRuleComponentProvider(
            IReadOnlyDictionary<Type, Func<object>> componentRegistrations,
            IReadOnlyDictionary<Type, object> singletonComponents)
        {
            _componentRegistrations = componentRegistrations;
            _singletonComponents = singletonComponents;
        }

        public bool TryGetComponentInstance(Type componentType, out object component)
        {
            if (_singletonComponents.TryGetValue(componentType, out component))
            {
                return true;
            }

            if (_componentRegistrations.TryGetValue(componentType, out Func<object> componentFactory))
            {
                component = componentFactory();
                return true;
            }

            return false;
        }
    }

    public class RuleComponentProviderBuilder
    {
        private readonly Dictionary<Type, object> _singletonComponents;

        private readonly Dictionary<Type, Func<object>> _componentRegistrations;

        public RuleComponentProviderBuilder()
        {
            _singletonComponents = new Dictionary<Type, object>();
            _componentRegistrations = new Dictionary<Type, Func<object>>();
        }

        public RuleComponentProviderBuilder AddSingleton<T>() where T : new()
        {
            _singletonComponents[typeof(T)] = new T();
            return this;
        }

        public RuleComponentProviderBuilder AddSingleton<T>(T instance)
        {
            _singletonComponents[typeof(T)] = instance;
            return this;
        }

        public RuleComponentProviderBuilder AddSingleton<TRegistered, TInstance>() where TInstance : TRegistered, new()
        {
            _singletonComponents[typeof(TRegistered)] = new TInstance();
            return this;
        }

        public RuleComponentProviderBuilder AddSingleton<TRegistered, TInstance>(TInstance instance)
        {
            _singletonComponents[typeof(TRegistered)] = instance;
            return this;
        }

        public RuleComponentProviderBuilder AddSingleton(Type registeredType, object instance)
        {
            if (!registeredType.IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException($"Cannot register object '{instance}' of type '{instance.GetType()}' for type '{registeredType}'");
            }

            _singletonComponents[registeredType] = instance;
            return this;
        }

        public RuleComponentProviderBuilder AddScoped<T>() where T : new()
        {
            _componentRegistrations[typeof(T)] = () => new T();
            return this;
        }

        public RuleComponentProviderBuilder AddScoped<T>(Func<T> factory) where T : class
        {
            _componentRegistrations[typeof(T)] = factory;
            return this;
        }

        public RuleComponentProviderBuilder AddScoped<TRegistered, TInstance>() where TInstance : TRegistered, new()
        {
            _componentRegistrations[typeof(TRegistered)] = () => new TInstance();
            return this;
        }

        public RuleComponentProviderBuilder AddScoped<TRegistered, TInstance>(Func<TInstance> factory) where TInstance : class, TRegistered 
        {
            _componentRegistrations[typeof(TRegistered)] = factory;
            return this;
        }

        public IRuleComponentProvider Build()
        {
            return new SimpleRuleComponentProvider(_componentRegistrations, _singletonComponents);
        }
    }
}
