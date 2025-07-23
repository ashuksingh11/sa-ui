using System;
using System.Collections.Generic;

namespace SamsungAccountUI.Services.Container
{
    /// <summary>
    /// Manual dependency injection container for Tizen compatibility
    /// Replaces Microsoft.Extensions.DependencyInjection which is not available on Tizen
    /// </summary>
    public class TizenServiceContainer
    {
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<TizenServiceContainer, object>> _serviceFactories = new Dictionary<Type, Func<TizenServiceContainer, object>>();
        private readonly Dictionary<Type, ServiceLifetime> _serviceLifetimes = new Dictionary<Type, ServiceLifetime>();

        /// <summary>
        /// Register a singleton service - one instance for the entire application lifetime
        /// </summary>
        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _serviceFactories[typeof(TInterface)] = container => new TImplementation();
            _serviceLifetimes[typeof(TInterface)] = ServiceLifetime.Singleton;
        }

        /// <summary>
        /// Register a singleton service with custom factory
        /// </summary>
        public void RegisterSingleton<TInterface>(Func<TizenServiceContainer, TInterface> factory)
        {
            _serviceFactories[typeof(TInterface)] = container => factory(container);
            _serviceLifetimes[typeof(TInterface)] = ServiceLifetime.Singleton;
        }

        /// <summary>
        /// Register a transient service - new instance every time
        /// </summary>
        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _serviceFactories[typeof(TInterface)] = container => new TImplementation();
            _serviceLifetimes[typeof(TInterface)] = ServiceLifetime.Transient;
        }

        /// <summary>
        /// Register a transient service with custom factory
        /// </summary>
        public void RegisterTransient<TInterface>(Func<TizenServiceContainer, TInterface> factory)
        {
            _serviceFactories[typeof(TInterface)] = container => factory(container);
            _serviceLifetimes[typeof(TInterface)] = ServiceLifetime.Transient;
        }

        /// <summary>
        /// Get a service instance
        /// </summary>
        public T GetService<T>()
        {
            var serviceType = typeof(T);
            
            if (!_serviceFactories.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered");
            }

            var lifetime = _serviceLifetimes[serviceType];

            if (lifetime == ServiceLifetime.Singleton)
            {
                // Return existing instance or create new one
                if (!_singletonInstances.ContainsKey(serviceType))
                {
                    _singletonInstances[serviceType] = _serviceFactories[serviceType](this);
                }
                return (T)_singletonInstances[serviceType];
            }
            else // Transient
            {
                // Always create new instance
                return (T)_serviceFactories[serviceType](this);
            }
        }

        /// <summary>
        /// Try to get a service instance, returns null if not registered
        /// </summary>
        public T GetServiceOrNull<T>() where T : class
        {
            try
            {
                return GetService<T>();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public bool IsRegistered<T>()
        {
            return _serviceFactories.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Clear all singleton instances (useful for cleanup)
        /// </summary>
        public void ClearSingletons()
        {
            // Dispose any disposable singletons
            foreach (var instance in _singletonInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _singletonInstances.Clear();
        }

        /// <summary>
        /// Get all registered service types
        /// </summary>
        public IEnumerable<Type> GetRegisteredServiceTypes()
        {
            return _serviceFactories.Keys;
        }
    }

    /// <summary>
    /// Service lifetime enumeration
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// One instance for the entire application lifetime
        /// </summary>
        Singleton,
        
        /// <summary>
        /// New instance every time the service is requested
        /// </summary>
        Transient
    }
}