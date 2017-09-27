using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data
{
    public abstract class ModelExtension
    {
        private static MounterManager<ModelExtension, Column> s_columnManager = new MounterManager<ModelExtension, Column>();

        protected static void RegisterColumn<TExtension, TColumn>(Expression<Func<TExtension, TColumn>> getter, Action<TColumn> initializer = null)
            where TExtension : ModelExtension
            where TColumn : Column, new()
        {
            var columnAttributes = getter.Verify(nameof(getter));

            s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer, columnAttributes));
        }

        private static T CreateColumn<TExtension, T>(Mounter<TExtension, T> mounter, Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where TExtension : ModelExtension
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OwnerType, mounter.Name);
            var parent = mounter.Parent;
            result.Construct(parent.Model, mounter.OwnerType, parent.GetName(mounter), ColumnKind.Extension, null, initializer.Merge(columnAttributes));
            return result;
        }

        protected static void RegisterColumn<TExtension, TColumn>(Expression<Func<TExtension, TColumn>> getter,
            Mounter<TColumn> fromMounter,
            Action<TColumn> initializer = null)
            where TExtension : ModelExtension
            where TColumn : Column, new()
        {
            var columnAttributes = getter.Verify(nameof(getter));
            Utilities.Check.NotNull(fromMounter, nameof(fromMounter));

            var result = s_columnManager.Register(getter, mounter => CreateColumn(mounter, initializer, columnAttributes));
            result.OriginalOwnerType = fromMounter.OriginalOwnerType;
            result.OriginalName = fromMounter.OriginalName;
        }

        static MounterManager<ModelExtension, ModelExtension> s_childExtensionManager = new MounterManager<ModelExtension, ModelExtension>();

        protected static void RegisterChildModel<TExtension, TChild>(Expression<Func<TExtension, TChild>> getter)
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
            return result;
        }

        internal void Initialize(Model model, string name)
        {
            Debug.Assert(model != null);
            Model = model;
            FullName = name;
            Mount();
        }

        private void Initialize(ModelExtension parent, string name)
        {
            Debug.Assert(parent != null);
            Debug.Assert(parent.Model != null);
            Model = parent.Model;
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

        private string FullName { get; set; }

        private string GetName<T>(Mounter<T> mounter)
        {
            return FullName + "." + mounter.Name;
        }
    }
}
