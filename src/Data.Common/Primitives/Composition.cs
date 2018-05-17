using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    public abstract class Composition<T> : ColumnContainer
        where T : Projection
    {
        static MounterManager<Composition<T>, T> s_childManager = new MounterManager<Composition<T>, T>();

        protected static void RegisterChildContainer<TComposition, TChild>(Expression<Func<TComposition, TChild>> getter)
            where TComposition : Composition<T>
            where TChild : T, new()
        {
            Check.NotNull(getter, nameof(getter));
            s_childManager.Register(getter, CreateChildContainer, null);
        }

        private static TChild CreateChildContainer<TComposition, TChild>(Mounter<TComposition, TChild> mounter)
            where TComposition : Composition<T>
            where TChild : T, new()
        {
            TChild result = new TChild();
            var parent = mounter.Parent;
            result.Initialize(parent, mounter.Name);
            parent.Add(result);
            return result;
        }

        public sealed override IReadOnlyList<Column> Columns
        {
            get { return Array<Column>.Empty; }
        }

        public sealed override IReadOnlyDictionary<string, Column> ColumnsByRelativeName
        {
            get { return EmptyDictionary<string, Column>.Singleton; }
        }

        private sealed class ChildrenCollection : KeyedCollection<string, ColumnContainer>, IReadOnlyDictionary<string, ColumnContainer>
        {
            public IEnumerable<string> Keys
            {
                get
                {
                    foreach (var container in this)
                        yield return container.Name;
                }
            }

            public IEnumerable<ColumnContainer> Values
            {
                get { return this; }
            }

            public bool ContainsKey(string key)
            {
                return Contains(key);
            }

            public bool TryGetValue(string key, out ColumnContainer value)
            {
                if (Contains(key))
                {
                    value = this[key];
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            protected override string GetKeyForItem(ColumnContainer item)
            {
                return item.Name;
            }

            IEnumerator<KeyValuePair<string, ColumnContainer>> IEnumerable<KeyValuePair<string, ColumnContainer>>.GetEnumerator()
            {
                foreach (var container in this)
                    yield return new KeyValuePair<string, ColumnContainer>(container.Name, container);
            }
        }

        private ChildrenCollection _children;
        public sealed override IReadOnlyList<ColumnContainer> Children
        {
            get
            {
                if (_children == null)
                    return Array<ColumnContainer>.Empty;
                else
                    return _children;
            }
        }

        public sealed override IReadOnlyDictionary<string, ColumnContainer> ChildrenByName
        {
            get
            {
                if (_children == null)
                    return EmptyDictionary<string, ColumnContainer>.Singleton;
                else
                    return _children;
            }
        }

        private void Add(T child)
        {
            if (_children == null)
                _children = new ChildrenCollection();
            _children.Add(child);
        }

        internal sealed override void Mount()
        {
            s_childManager.Mount(this);
        }

        internal abstract Type CompositionType { get; }
    }
}
