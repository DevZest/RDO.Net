using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class LeafProjection<T> : LeafProjection
        where T : PrimaryKey
    {
        protected abstract T CreatePrimaryKey();

        protected sealed override PrimaryKey GetPrimaryKey()
        {
            return PrimaryKey;
        }

        private T _primaryKey;
        public new T PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = CreatePrimaryKey()); }
        }
    }

    public abstract class LeafProjection : Projection
    {
        private static MounterManager<LeafProjection, Column> s_columnManager = new MounterManager<LeafProjection, Column>();

        [MounterRegistration]
        protected static void Register<TProjection, TColumn>(Expression<Func<TProjection, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TProjection : LeafProjection
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            fromMounter.VerifyNotNull(nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static T CreateColumn<TProjection, T>(Mounter<TProjection, T> mounter, Action<T> initializer)
            where TProjection : LeafProjection
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OriginalDeclaringType, mounter.OriginalName);
            var parent = mounter.Parent;
            result.Construct(parent.Model, mounter.DeclaringType, parent.GetName(mounter), ColumnKind.ContainerProperty, null, initializer);
            parent.Add(result);
            return result;
        }

        private sealed class ColumnCollection : KeyedCollection<string, Column>, IReadOnlyDictionary<string, Column>
        {
            public IEnumerable<string> Keys
            {
                get
                {
                    foreach (var column in this)
                        yield return column.RelativeName;
                }
            }

            public IEnumerable<Column> Values
            {
                get { return this; }
            }

            public bool ContainsKey(string key)
            {
                return Contains(key);
            }

            public bool TryGetValue(string key, out Column value)
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

            protected override string GetKeyForItem(Column item)
            {
                return item.RelativeName;
            }

            IEnumerator<KeyValuePair<string, Column>> IEnumerable<KeyValuePair<string, Column>>.GetEnumerator()
            {
                foreach (var column in this)
                    yield return new KeyValuePair<string, Column>(column.RelativeName, column);
            }
        }

        private ColumnCollection _columns;
        public sealed override IReadOnlyList<Column> Columns
        {
            get
            {
                if (_columns == null)
                    return Array<Column>.Empty;
                else
                    return _columns;
            }
        }

        public sealed override IReadOnlyDictionary<string, Column> ColumnsByRelativeName
        {
            get
            {
                if (_columns == null)
                    return EmptyDictionary<string, Column>.Singleton;
                else
                    return _columns;
            }
        }

        public sealed override IReadOnlyList<Projection> Children
        {
            get { return Array<Projection>.Empty; }
        }

        public sealed override IReadOnlyDictionary<string, Projection> ChildrenByName
        {
            get { return EmptyDictionary<string, Projection>.Singleton; }
        }

        private void Add(Column column)
        {
            if (_columns == null)
                _columns = new ColumnCollection();
            _columns.Add(column);
        }

        internal sealed override void Mount()
        {
            s_columnManager.Mount(this);
        }

        public PrimaryKey PrimaryKey
        {
            get { return GetPrimaryKey(); }
        }

        protected virtual PrimaryKey GetPrimaryKey()
        {
            return null;
        }
    }
}
