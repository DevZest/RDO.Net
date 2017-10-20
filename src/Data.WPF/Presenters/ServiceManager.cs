using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Presenters
{
    public static class ServiceManager
    {
        private static Dictionary<Type, Func<IService>> _serviceProviders = new Dictionary<Type, Func<IService>>();
        private static ConditionalWeakTable<DataPresenter, ConcurrentDictionary<Type, IService>> _services = new ConditionalWeakTable<DataPresenter, ConcurrentDictionary<Type, IService>>();

        public static void Register<TService, TServiceImpl>()
            where TService : class, IService
            where TServiceImpl : TService, new()
        {
            var serviceType = typeof(TService);
            if (_serviceProviders.ContainsKey(serviceType))
                throw new InvalidOperationException();
            _serviceProviders.Add(serviceType, () => new TServiceImpl());
        }

        public static T GetService<T>(DataPresenter dataPresenter)
            where T : class, IService
        {
            if (dataPresenter == null)
                throw new ArgumentNullException(nameof(dataPresenter));

            var servicesByType = _services.GetOrCreateValue(dataPresenter);
            return (T)servicesByType.GetOrAdd(typeof(T), type => CreateService<T>(dataPresenter));
        }

        private static T CreateService<T>(DataPresenter dataPresenter)
            where T : class, IService
        {
            Func<IService> constructor;
            if (_serviceProviders.TryGetValue(typeof(T), out constructor))
            {
                var result = (T)constructor();
                result.Initialize(dataPresenter);
                return result;
            }
            return null;
        }
    }
}
