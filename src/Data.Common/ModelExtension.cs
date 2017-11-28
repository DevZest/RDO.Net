using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class ModelExtension
    {
        private static MounterManager<ModelExtension, Column> s_columnManager = new MounterManager<ModelExtension, Column>();

        protected static void RegisterColumn<TExtension, TColumn>(Expression<Func<TExtension, TColumn>> getter)
            where TExtension : ModelExtension
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = result.DeclaringType;
            result.OriginalName = result.Name;
        }

        private static T CreateColumn<TExtension, T>(Mounter<TExtension, T> mounter, Action<T> initializer)
            where TExtension : ModelExtension
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OriginalDeclaringType, mounter.OriginalName);
            var parent = mounter.Parent;
            result.Construct(parent.Model, mounter.DeclaringType, parent.GetName(mounter), ColumnKind.Extension, null, initializer);
            parent.Add(result);
            return result;
        }

        protected static void RegisterColumn<TExtension, TColumn>(Expression<Func<TExtension, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TExtension : ModelExtension
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            Utilities.Check.NotNull(fromMounter, nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        static MounterManager<ModelExtension, ModelExtension> s_childExtensionManager = new MounterManager<ModelExtension, ModelExtension>();

        protected static void RegisterChildExtension<TExtension, TChild>(Expression<Func<TExtension, TChild>> getter)
            where TExtension : ModelExtension, new()
            where TChild : ModelExtension, new()
        {
            Check.NotNull(getter, nameof(getter));
            s_childExtensionManager.Register(getter, CreateChildExtension, null);
        }

        private static TChild CreateChildExtension<TExtension, TChild>(Mounter<TExtension, TChild> mounter)
            where TExtension : ModelExtension, new()
            where TChild : ModelExtension, new()
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
            Name = FullName = nameof(Model.Extension);
            Mount();
        }

        private void Initialize(ModelExtension parent, string name)
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
            Mount(s_childExtensionManager);
            OnMounted();
        }

        protected virtual void OnMounted()
        {
        }

        private void Mount<T>(MounterManager<ModelExtension, T> mounterManager)
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

        private sealed class ExtensionCollection : KeyedCollection<string, ModelExtension>, IReadOnlyDictionary<string, ModelExtension>
        {
            public IEnumerable<string> Keys
            {
                get
                {
                    foreach (var extension in this)
                        yield return extension.Name;
                }
            }

            public IEnumerable<ModelExtension> Values
            {
                get { return this; }
            }

            public bool ContainsKey(string key)
            {
                return Contains(key);
            }

            public bool TryGetValue(string key, out ModelExtension value)
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

            protected override string GetKeyForItem(ModelExtension item)
            {
                return item.Name;
            }

            IEnumerator<KeyValuePair<string, ModelExtension>> IEnumerable<KeyValuePair<string, ModelExtension>>.GetEnumerator()
            {
                foreach (var extension in this)
                    yield return new KeyValuePair<string, ModelExtension>(extension.Name, extension);
            }
        }

        private ExtensionCollection _childExtensions;
        public IReadOnlyList<ModelExtension> ChildExtensions
        {
            get
            {
                if (_childExtensions == null)
                    return Array<ModelExtension>.Empty;
                else
                    return _childExtensions;
            }
        }

        public IReadOnlyDictionary<string, ModelExtension> ChildExtensionsByName
        {
            get
            {
                if (_childExtensions == null)
                    return EmptyDictionary<string, ModelExtension>.Singleton;
                else
                    return _childExtensions;
            }
        }

        private void Add(ModelExtension childExtension)
        {
            if (_childExtensions == null)
                _childExtensions = new ExtensionCollection();
            _childExtensions.Add(childExtension);
        }
    }
}
