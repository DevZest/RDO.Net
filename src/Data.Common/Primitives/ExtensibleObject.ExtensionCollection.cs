using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Primitives
{
    partial class ExtensibleObject
    {
        private sealed class ExtensionCollection : KeyedCollection<object, IExtension>
        {
            private static class Cache<T>
                where T : class, IExtension
            {
                private static ConditionalWeakTable<ExtensionCollection, ReadOnlyCollection<T>> s_results = new ConditionalWeakTable<ExtensionCollection, ReadOnlyCollection<T>>();

                public static void Remove(ExtensionCollection collection)
                {
                    ReadOnlyCollection<T> results;
                    if (s_results.TryGetValue(collection, out results))
                        s_results.Remove(collection);
                }

                public static ReadOnlyCollection<T> GetResult(ExtensionCollection collection)
                {
                    return s_results.GetValue(collection, x => new ReadOnlyCollection<T>(Filter(x).ToArray()));
                }

                private static IEnumerable<T> Filter(ExtensionCollection collection)
                {
                    foreach (var item in collection)
                    {
                        var result = item as T;
                        if (result != null)
                            yield return result;
                    }
                }
            }

            public ExtensionCollection(ExtensibleObject interceptable)
            {
                _designable = interceptable as IDesignable;
            }

            private IDesignable _designable;
            private bool IsFrozen
            {
                get { return _allowFrozenChange ? false : (_designable == null ? false : !_designable.DesignMode); }
            }

            bool _allowFrozenChange;
            internal void AllowFrozenChange(bool value)
            {
                _allowFrozenChange = value;
            }

            protected override object GetKeyForItem(IExtension item)
            {
                return item.Key;
            }

            protected override void InsertItem(int index, IExtension item)
            {
                OnItemChanging(item);
                base.InsertItem(index, item);
            }

            protected override void ClearItems()
            {
                Debug.Fail("Not supported.");
            }

            protected override void RemoveItem(int index)
            {
                OnItemChanging(((Collection<IExtension>)this)[index]);
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, IExtension item)
            {
                Debug.Fail("Not supported.");
            }

            private void OnItemChanging(IExtension item)
            {
                if (IsFrozen)
                    throw new InvalidOperationException(Strings.VerifyDesignMode);

                var type = item.GetType();
                var invalidatedTypes = new List<Type>();
                foreach (var cachedType in _cacheInvalidators.Keys)
                {
                    if (cachedType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    {
                        _cacheInvalidators[cachedType]();
                        invalidatedTypes.Add(cachedType);
                    }
                }

                foreach (var invalidatedType in invalidatedTypes)
                    _cacheInvalidators.Remove(invalidatedType);
            }

            Dictionary<Type, Action> _cacheInvalidators = new Dictionary<Type, Action>();
            private void OnCacheAccessing<T>()
                where T : class, IExtension
            {
                var type = typeof(T);
                if (_cacheInvalidators.ContainsKey(type))
                    return;
                _cacheInvalidators.Add(type, () => Cache<T>.Remove(this));
            }

            public ReadOnlyCollection<T> Filter<T>()
                where T : class, IExtension
            {
                OnCacheAccessing<T>();
                return Cache<T>.GetResult(this);
            }
        }
    }
}
