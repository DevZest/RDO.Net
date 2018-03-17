using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

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
            _serviceProviders.Add(serviceType, () => new TServiceImpl());
        }

        public static bool IsRegistered<TService>()
            where TService : class, IService
        {
            return _serviceProviders.ContainsKey(typeof(TService));
        }

        public static T GetService<T>(DataPresenter dataPresenter, bool autoCreate = true)
            where T : class, IService
        {
            if (dataPresenter == null)
                throw new ArgumentNullException(nameof(dataPresenter));

            if (autoCreate)
            {
                var servicesByType = _services.GetOrCreateValue(dataPresenter);
                return (T)servicesByType.GetOrAdd(typeof(T), type => CreateService<T>(dataPresenter));
            }
            else if (_services.TryGetValue(dataPresenter, out var servicesByType))
            {
                if (servicesByType.ContainsKey(typeof(T)))
                    return (T)servicesByType[typeof(T)];
            }

            return null;
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

        private sealed class ServiceIdentifier
        {
            public ServiceIdentifier(IService service, Type type)
            {
                Debug.Assert(service != null);
                Debug.Assert(type != null);
                Service = service;
                Type = type;
            }

            public readonly IService Service;
            public readonly Type Type;
        }

        private sealed class ServiceIdentifierBag : KeyedCollection<Type, ServiceIdentifier>
        {
            protected override Type GetKeyForItem(ServiceIdentifier item)
            {
                return item.Type;
            }
        }

        private static ConditionalWeakTable<object, ServiceIdentifierBag> s_serviceIdentifiers = new ConditionalWeakTable<object, ServiceIdentifierBag>();

        private static ServiceIdentifier GetServiceIdentifier(this IService service, Type type)
        {
            Debug.Assert(service != null);
            Debug.Assert(type != null);

            ServiceIdentifierBag bag = s_serviceIdentifiers.GetOrCreateValue(service);
            if (bag.Contains(type))
                return bag[type];
            else
            {
                var result = new ServiceIdentifier(service, type);
                bag.Add(result);
                return result;
            }
        }

        private static bool Setup(this UIElement element, ServiceIdentifier serviceIdentifier)
        {
            var bag = s_serviceIdentifiers.GetOrCreateValue(element);
            if (bag.Contains(serviceIdentifier.Type))
                return false;

            bag.Add(serviceIdentifier);
            return true;
        }

        private static bool Cleanup(this UIElement element, ServiceIdentifier serviceIdentifier)
        {
            ServiceIdentifierBag bag;
            if (!s_serviceIdentifiers.TryGetValue(element, out bag))
                return false;

            return bag.Remove(serviceIdentifier);
        }

        private sealed class CommandBindingEx : CommandBinding
        {
            public CommandBindingEx(ServiceIdentifier serviceIdentifier, ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
                : base(command, executed, canExecute)
            {
                Debug.Assert(serviceIdentifier != null);
                ServiceIdentifier = serviceIdentifier;
            }

            public readonly ServiceIdentifier ServiceIdentifier;
        }

        private sealed class InputBindingEx : InputBinding
        {
            public InputBindingEx(ServiceIdentifier serviceIdentifier, ICommand command, InputGesture inputGesture)
                : base(command, inputGesture)
            {
                Debug.Assert(serviceIdentifier != null);
                ServiceIdentifier = serviceIdentifier;
            }

            public readonly ServiceIdentifier ServiceIdentifier;
        }

        public static void Setup<T>(this ICommandService<T> commandService,  T element)
            where T : UIElement
        {
            Check.NotNull(element, nameof(element));

            var serviceIdentifier = commandService.GetServiceIdentifier(typeof(ICommandService<T>));
            if (!element.Setup(serviceIdentifier))
                return;

            var commandEntries = commandService.GetCommandEntries(element);
            if (commandEntries == null)
                return;
            foreach (var entry in commandEntries)
            {
                if (entry.Executed != null)
                {
                    var commandBinding = new CommandBindingEx(serviceIdentifier, entry.Command, entry.Executed, entry.CanExecute);
                    element.CommandBindings.Add(commandBinding);
                }
                for (int i = 0; i < entry.InputGesturesCount; i++)
                {
                    var inputBinding = new InputBindingEx(serviceIdentifier, entry.Command, entry.GetInputGesture(i));
                    element.InputBindings.Add(inputBinding);
                }
            }
        }

        public static void Cleanup<T>(this ICommandService<T> commandService, T element)
            where T : UIElement
        {
            var serviceIdentifier = commandService.GetServiceIdentifier(typeof(ICommandService<T>));
            if (!element.Cleanup(serviceIdentifier))
                return; ;

            CleanupCommandBindings(element.CommandBindings, serviceIdentifier);
            CleanupInputBindings(element.InputBindings, serviceIdentifier);
        }

        private static void CleanupCommandBindings(CommandBindingCollection commandBindings, ServiceIdentifier serviceIdentifier)
        {
            for (int i = commandBindings.Count - 1; i >= 0; i--)
            {
                var commandBindingEx = commandBindings[i] as CommandBindingEx;
                if (commandBindingEx != null && commandBindingEx.ServiceIdentifier == serviceIdentifier)
                    commandBindings.RemoveAt(i);
            }
        }

        private static void CleanupInputBindings(InputBindingCollection inputBindings, ServiceIdentifier serviceIdentifier)
        {
            for (int i = inputBindings.Count - 1; i >= 0; i--)
            {
                var inputBindingEx = inputBindings[i] as InputBindingEx;
                if (inputBindingEx != null && inputBindingEx.ServiceIdentifier == serviceIdentifier)
                    inputBindings.RemoveAt(i);
            }
        }
    }
}
