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

        private static bool Cleanup(this UIElement element)
        {
            return s_serviceIdentifiers.Remove(element);
        }

        private sealed class CommandBindingEx : CommandBinding
        {
            public CommandBindingEx(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
                : base(command, executed, canExecute)
            {
            }
        }

        private sealed class InputBindingEx : InputBinding
        {
            public InputBindingEx(ICommand command, InputGesture inputGesture)
                : base(command, inputGesture)
            {
            }
        }

        public static void SetupCommandEntries<TService, TElement>(this TElement element, TService commandService,  Func<TService, TElement, IEnumerable<CommandEntry>> getCommandEntries)
            where TService : IService
            where TElement : UIElement
        {
            Check.NotNull(element, nameof(element));
            if (commandService == null)
                throw new ArgumentNullException(nameof(commandService));
            Check.NotNull(getCommandEntries, nameof(getCommandEntries));

            var serviceIdentifier = commandService.GetServiceIdentifier(typeof(TService));
            if (!element.Setup(serviceIdentifier))
                return;

            var commandEntries = getCommandEntries(commandService, element);
            if (commandEntries == null)
                return;
            foreach (var entry in commandEntries)
            {
                if (entry.Executed != null)
                {
                    var commandBinding = new CommandBindingEx(entry.Command, entry.Executed, entry.CanExecute);
                    element.CommandBindings.Add(commandBinding);
                }
                for (int i = 0; i < entry.InputGesturesCount; i++)
                {
                    var inputBinding = new InputBindingEx(entry.Command, entry.GetInputGesture(i));
                    element.InputBindings.Add(inputBinding);
                }
            }
        }

        public static void CleanupCommandEntries(this UIElement element)
        {
            Check.NotNull(element, nameof(element));

            if (!element.Cleanup())
                return;

            CleanupCommandBindings(element.CommandBindings);
            CleanupInputBindings(element.InputBindings);
        }

        private static void CleanupCommandBindings(CommandBindingCollection commandBindings)
        {
            for (int i = commandBindings.Count - 1; i >= 0; i--)
            {
                var commandBindingEx = commandBindings[i] as CommandBindingEx;
                if (commandBindingEx != null)
                    commandBindings.RemoveAt(i);
            }
        }

        private static void CleanupInputBindings(InputBindingCollection inputBindings)
        {
            for (int i = inputBindings.Count - 1; i >= 0; i--)
            {
                var inputBindingEx = inputBindings[i] as InputBindingEx;
                if (inputBindingEx != null)
                    inputBindings.RemoveAt(i);
            }
        }
    }
}
