using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Primitives
{
    partial class ResourceContainer
    {
        private sealed class ResourceCollection : KeyedCollection<object, IResource>
        {
            private static class Cache<T>
                where T : class, IResource
            {
                private static ConditionalWeakTable<ResourceCollection, ReadOnlyCollection<T>> s_results = new ConditionalWeakTable<ResourceCollection, ReadOnlyCollection<T>>();

                public static void Remove(ResourceCollection collection)
                {
                    ReadOnlyCollection<T> results;
                    if (s_results.TryGetValue(collection, out results))
                        s_results.Remove(collection);
                }

                public static ReadOnlyCollection<T> GetResult(ResourceCollection collection)
                {
                    return s_results.GetValue(collection, x => new ReadOnlyCollection<T>(Filter(x).ToArray()));
                }

                private static IEnumerable<T> Filter(ResourceCollection collection)
                {
                    foreach (var item in collection)
                    {
                        var result = item as T;
                        if (result != null)
                            yield return result;
                    }
                }
            }

            public ResourceCollection(ResourceContainer interceptable)
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

            protected override object GetKeyForItem(IResource item)
            {
                return item.Key;
            }

            protected override void InsertItem(int index, IResource item)
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
                OnItemChanging(((Collection<IResource>)this)[index]);
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, IResource item)
            {
                Debug.Fail("Not supported.");
            }

            private void OnItemChanging(IResource item)
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
                where T : class, IResource
            {
                var type = typeof(T);
                if (_cacheInvalidators.ContainsKey(type))
                    return;
                _cacheInvalidators.Add(type, () => Cache<T>.Remove(this));
            }

            public ReadOnlyCollection<T> Filter<T>()
                where T : class, IResource
            {
                OnCacheAccessing<T>();
                return Cache<T>.GetResult(this);
            }
        }
    }
}
