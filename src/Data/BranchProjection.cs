using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class BranchProjection : Projection
    {
        static MounterManager<BranchProjection, Projection> s_childManager = new MounterManager<BranchProjection, Projection>();

        [MounterRegistration]
        protected static void Register<TParent, TChild>(Expression<Func<TParent, TChild>> getter)
            where TParent : BranchProjection
            where TChild : Projection, new()
        {
            getter.VerifyNotNull(nameof(getter));
            s_childManager.Register(getter, CreateChild, null);
        }

        private static TChild CreateChild<TParent, TChild>(Mounter<TParent, TChild> mounter)
            where TParent : BranchProjection
            where TChild : Projection, new()
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

        private sealed class ChildrenCollection : KeyedCollection<string, Projection>, IReadOnlyDictionary<string, Projection>
        {
            public IEnumerable<string> Keys
            {
                get
                {
                    foreach (var container in this)
                        yield return container.Name;
                }
            }

            public IEnumerable<Projection> Values
            {
                get { return this; }
            }

            public bool ContainsKey(string key)
            {
                return Contains(key);
            }

            public bool TryGetValue(string key, out Projection value)
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

            protected override string GetKeyForItem(Projection item)
            {
                return item.Name;
            }

            IEnumerator<KeyValuePair<string, Projection>> IEnumerable<KeyValuePair<string, Projection>>.GetEnumerator()
            {
                foreach (var container in this)
                    yield return new KeyValuePair<string, Projection>(container.Name, container);
            }
        }

        private ChildrenCollection _children;
        public sealed override IReadOnlyList<Projection> Children
        {
            get
            {
                if (_children == null)
                    return Array<Projection>.Empty;
                else
                    return _children;
            }
        }

        public sealed override IReadOnlyDictionary<string, Projection> ChildrenByName
        {
            get
            {
                if (_children == null)
                    return EmptyDictionary<string, Projection>.Singleton;
                else
                    return _children;
            }
        }

        private void Add(Projection child)
        {
            if (_children == null)
                _children = new ChildrenCollection();
            _children.Add(child);
        }

        internal sealed override void Mount()
        {
            s_childManager.Mount(this);
        }
    }
}
