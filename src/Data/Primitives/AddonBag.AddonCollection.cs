using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DevZest.Data.Addons;

namespace DevZest.Data.Primitives
{
    partial class AddonBag
    {
        private sealed class AddonCollection : KeyedCollection<object, IAddon>
        {
            private static class Cache<T>
                where T : class, IAddon
            {
                private static ConditionalWeakTable<AddonCollection, ReadOnlyCollection<T>> s_results = new ConditionalWeakTable<AddonCollection, ReadOnlyCollection<T>>();

                public static void Remove(AddonCollection collection)
                {
                    ReadOnlyCollection<T> results;
                    if (s_results.TryGetValue(collection, out results))
                        s_results.Remove(collection);
                }

                public static ReadOnlyCollection<T> GetResult(AddonCollection collection)
                {
                    return s_results.GetValue(collection, x => new ReadOnlyCollection<T>(Filter(x).ToArray()));
                }

                private static IEnumerable<T> Filter(AddonCollection collection)
                {
                    foreach (var item in collection)
                    {
                        if (item is T result)
                            yield return result;
                    }
                }
            }

            public AddonCollection(AddonBag addonBag)
            {
                _sealable = addonBag as ISealable;
            }

            private ISealable _sealable;
            private bool IsSealed
            {
                get { return _allowSealedChange ? false : (_sealable == null ? false : _sealable.IsSealed); }
            }

            bool _allowSealedChange;
            internal void AllowSealedChange(bool value)
            {
                _allowSealedChange = value;
            }

            protected override object GetKeyForItem(IAddon item)
            {
                return item.Key;
            }

            protected override void InsertItem(int index, IAddon item)
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
                OnItemChanging(((Collection<IAddon>)this)[index]);
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, IAddon item)
            {
                Debug.Fail("Not supported.");
            }

            private void OnItemChanging(IAddon item)
            {
                if (IsSealed)
                    throw new InvalidOperationException(DiagnosticMessages.Common_VerifyDesignMode);

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
                where T : class, IAddon
            {
                var type = typeof(T);
                if (_cacheInvalidators.ContainsKey(type))
                    return;
                _cacheInvalidators.Add(type, () => Cache<T>.Remove(this));
            }

            public ReadOnlyCollection<T> Filter<T>()
                where T : class, IAddon
            {
                OnCacheAccessing<T>();
                return Cache<T>.GetResult(this);
            }
        }
    }
}
