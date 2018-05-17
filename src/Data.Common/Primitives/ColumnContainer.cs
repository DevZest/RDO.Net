using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnContainer : IModelReference
    {
        internal const string EXT_ROOT_NAME = "__Ext";

        internal void Initialize(Model model)
        {
            Debug.Assert(model != null);
            Model = model;
            Name = FullName = model.IsExtRoot ? string.Empty : EXT_ROOT_NAME;
            Mount();
        }

        internal void Initialize(ColumnContainer parent, string name)
        {
            Debug.Assert(parent != null);
            Debug.Assert(parent.Model != null);
            Model = parent.Model;
            Name = name;
            FullName = string.IsNullOrEmpty(parent.FullName) ? name : parent.FullName + "." + name;
            Mount();
        }

        internal abstract void Mount();

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

        internal string GetName<T>(Mounter<T> mounter)
        {
            return FullName + "." + mounter.Name;
        }

        public abstract IReadOnlyList<Column> Columns { get; }

        public abstract IReadOnlyDictionary<string, Column> ColumnsByRelativeName { get; }

        public abstract IReadOnlyList<ColumnContainer> Children { get; }

        public abstract IReadOnlyDictionary<string, ColumnContainer> ChildrenByName { get; }
    }
}
