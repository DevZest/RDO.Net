using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class ModelExtender
    {
        private static MounterManager<ModelExtender, Column> s_columnManager = new MounterManager<ModelExtender, Column>();
        internal const string ROOT_NAME = "__Ext";

        protected static void RegisterColumn<TExtender, TColumn>(Expression<Func<TExtender, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TExtender : ModelExtender
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            Utilities.Check.NotNull(fromMounter, nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static T CreateColumn<TExtender, T>(Mounter<TExtender, T> mounter, Action<T> initializer)
            where TExtender : ModelExtender
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OriginalDeclaringType, mounter.OriginalName);
            var parent = mounter.Parent;
            result.Construct(parent.Model, mounter.DeclaringType, parent.GetName(mounter), ColumnKind.Extender, null, initializer);
            parent.Add(result);
            return result;
        }

        static MounterManager<ModelExtender, ModelExtender> s_childExtenderManager = new MounterManager<ModelExtender, ModelExtender>();

        protected static void RegisterChildExtender<TExtender, TChild>(Expression<Func<TExtender, TChild>> getter)
            where TExtender : ModelExtender, new()
            where TChild : ModelExtender, new()
        {
            Check.NotNull(getter, nameof(getter));
            s_childExtenderManager.Register(getter, CreateChildExtender, null);
        }

        private static TChild CreateChildExtender<TExtender, TChild>(Mounter<TExtender, TChild> mounter)
            where TExtender : ModelExtender, new()
            where TChild : ModelExtender, new()
        {
            TChild result = new TChild();
            var parent = mounter.Parent;
            result.Initialize(parent, mounter.Name);
            parent.Add(result);
            return result;
        }

        internal void Initialize(Model model)
        {
            Debug.Assert(model != null);
            Model = model;
            Name = FullName = ROOT_NAME;
            Mount();
        }

        private void Initialize(ModelExtender parent, string name)
        {
            Debug.Assert(parent != null);
            Debug.Assert(parent.Model != null);
            Model = parent.Model;
            Name = name;
            FullName = parent.FullName + "." + name;
            Mount();
        }

        private void Mount()
        {
            Mount(s_columnManager);
            Mount(s_childExtenderManager);
            OnMounted();
        }

        protected virtual void OnMounted()
        {
        }

        private void Mount<T>(MounterManager<ModelExtender, T> mounterManager)
            where T : class
        {
            var mounters = mounterManager.GetAll(this.GetType());
            foreach (var mounter in mounters)
                mounter.Mount(this);
        }

        private Model Model { get; set; }

        internal string FullName { get; private set; }

        internal string Name { get; private set; }

        private string GetName<T>(Mounter<T> mounter)
        {
            return FullName + "." + mounter.Name;
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
        public IReadOnlyList<Column> Columns
        {
            get
            {
                if (_columns == null)
                    return Array<Column>.Empty;
                else
                    return _columns;
            }
        }

        public IReadOnlyDictionary<string, Column> ColumnsByRelativeName
        {
            get
            {
                if (_columns == null)
                    return EmptyDictionary<string, Column>.Singleton;
                else
                    return _columns;
            }
        }

        private void Add(Column column)
        {
            if (_columns == null)
                _columns = new ColumnCollection();
            _columns.Add(column);
        }

        private sealed class ExtenderCollection : KeyedCollection<string, ModelExtender>, IReadOnlyDictionary<string, ModelExtender>
        {
            public IEnumerable<string> Keys
            {
                get
                {
                    foreach (var extender in this)
                        yield return extender.Name;
                }
            }

            public IEnumerable<ModelExtender> Values
            {
                get { return this; }
            }

            public bool ContainsKey(string key)
            {
                return Contains(key);
            }

            public bool TryGetValue(string key, out ModelExtender value)
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

            protected override string GetKeyForItem(ModelExtender item)
            {
                return item.Name;
            }

            IEnumerator<KeyValuePair<string, ModelExtender>> IEnumerable<KeyValuePair<string, ModelExtender>>.GetEnumerator()
            {
                foreach (var extender in this)
                    yield return new KeyValuePair<string, ModelExtender>(extender.Name, extender);
            }
        }

        private ExtenderCollection _childExtenders;
        public IReadOnlyList<ModelExtender> ChildExtenders
        {
            get
            {
                if (_childExtenders == null)
                    return Array<ModelExtender>.Empty;
                else
                    return _childExtenders;
            }
        }

        public IReadOnlyDictionary<string, ModelExtender> ChildExtendersByName
        {
            get
            {
                if (_childExtenders == null)
                    return EmptyDictionary<string, ModelExtender>.Singleton;
                else
                    return _childExtenders;
            }
        }

        private void Add(ModelExtender childExtender)
        {
            if (_childExtenders == null)
                _childExtenders = new ExtenderCollection();
            _childExtenders.Add(childExtender);
        }
    }
}
