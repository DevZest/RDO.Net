using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    public abstract class Projection : ColumnCombination
    {
        private static MounterManager<Projection, Column> s_columnManager = new MounterManager<Projection, Column>();

        protected static void RegisterColumn<TProjection, TColumn>(Expression<Func<TProjection, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TProjection : Projection
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            Utilities.Check.NotNull(fromMounter, nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static T CreateColumn<TProjection, T>(Mounter<TProjection, T> mounter, Action<T> initializer)
            where TProjection : Projection
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

        public sealed override IReadOnlyList<ColumnCombination> Children
        {
            get { return Array<ColumnCombination>.Empty; }
        }

        public sealed override IReadOnlyDictionary<string, ColumnCombination> ChildrenByName
        {
            get { return EmptyDictionary<string, ColumnCombination>.Singleton; }
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

        internal abstract void PreventExternalAssemblyInheritance();
    }
}
