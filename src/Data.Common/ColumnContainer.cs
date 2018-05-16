using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class ColumnContainer : IModelReference
    {
        private static MounterManager<ColumnContainer, Column> s_columnManager = new MounterManager<ColumnContainer, Column>();
        internal const string EXT_ROOT_NAME = "__Ext";

        protected static void RegisterColumn<TContainer, TColumn>(Expression<Func<TContainer, TColumn>> getter, Mounter<TColumn> fromMounter)
            where TContainer : ColumnContainer
            where TColumn : Column, new()
        {
            var initializer = getter.Verify(nameof(getter));
            Utilities.Check.NotNull(fromMounter, nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer));
            result.OriginalDeclaringType = fromMounter.OriginalDeclaringType;
            result.OriginalName = fromMounter.OriginalName;
        }

        private static T CreateColumn<TContainer, T>(Mounter<TContainer, T> mounter, Action<T> initializer)
            where TContainer : ColumnContainer
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OriginalDeclaringType, mounter.OriginalName);
            var parent = mounter.Parent;
            result.Construct(parent.Model, mounter.DeclaringType, parent.GetName(mounter), ColumnKind.ContainerProperty, null, initializer);
            parent.Add(result);
            return result;
        }

        static MounterManager<ColumnContainer, ColumnContainer> s_childContainerManager = new MounterManager<ColumnContainer, ColumnContainer>();

        protected static void RegisterChildContainer<TContainer, TChild>(Expression<Func<TContainer, TChild>> getter)
            where TContainer : ColumnContainer
            where TChild : ColumnContainer, new()
        {
            Check.NotNull(getter, nameof(getter));
            s_childContainerManager.Register(getter, CreateChildContainer, null);
        }

        private static TChild CreateChildContainer<TContainer, TChild>(Mounter<TContainer, TChild> mounter)
            where TContainer : ColumnContainer
            where TChild : ColumnContainer, new()
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
            Name = FullName = model.IsExtRoot ? string.Empty : EXT_ROOT_NAME;
            Mount();
        }

        private void Initialize(ColumnContainer parent, string name)
        {
            Debug.Assert(parent != null);
            Debug.Assert(parent.Model != null);
            Model = parent.Model;
            Name = name;
            FullName = string.IsNullOrEmpty(parent.FullName) ? name : parent.FullName + "." + name;
            Mount();
        }

        private void Mount()
        {
            Mount(s_columnManager);
            Mount(s_childContainerManager);
            OnMounted();
        }

        protected virtual void OnMounted()
        {
        }

        private void Mount<T>(MounterManager<ColumnContainer, T> mounterManager)
            where T : class
        {
            var mounters = mounterManager.GetAll(this.GetType());
            foreach (var mounter in mounters)
                mounter.Mount(this);
        }

        private Model _model;
        public Model Model
        {
            get
            {
                EnsureInitialized();
                return _model;
            }
            private set { _model = value; }
        }

        private sealed class ContainerModel : Model
        {
            public ContainerModel(ColumnContainer ext)
            {
                Debug.Assert(ext != null && ext._model == null);
                ExtraColumns = ext;
            }

            internal override bool IsExtRoot
            {
                get { return true; }
            }
        }

        private void EnsureInitialized()
        {
            if (_model == null)
            {
                var containerModel = new ContainerModel(this);
                Debug.Assert(_model == containerModel);
            }
        }

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

        private sealed class ContainerCollection : KeyedCollection<string, ColumnContainer>, IReadOnlyDictionary<string, ColumnContainer>
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

        private ContainerCollection _childContainers;
        public IReadOnlyList<ColumnContainer> ChildContainers
        {
            get
            {
                if (_childContainers == null)
                    return Array<ColumnContainer>.Empty;
                else
                    return _childContainers;
            }
        }

        public IReadOnlyDictionary<string, ColumnContainer> ChildContainersByName
        {
            get
            {
                if (_childContainers == null)
                    return EmptyDictionary<string, ColumnContainer>.Singleton;
                else
                    return _childContainers;
            }
        }

        private void Add(ColumnContainer childContainer)
        {
            if (_childContainers == null)
                _childContainers = new ContainerCollection();
            _childContainers.Add(childContainer);
        }
    }
}
