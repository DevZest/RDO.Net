using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class TemplateItemBuilderFactory
    {
        internal TemplateItemBuilderFactory(TemplateBuilder templateBuilder, GridRange gridRange)
        {
            TemplateBuilder = templateBuilder;
            GridRange = gridRange;
        }

        internal readonly TemplateBuilder TemplateBuilder;

        internal readonly GridRange GridRange;

        internal int AutoSizeMeasureOrder { get; private set; }

        public TemplateItemBuilderFactory MeasureAutoSize(int order)
        {
            AutoSizeMeasureOrder = order;
            return this;
        }

        internal Template Template
        {
            get { return TemplateBuilder.Template; }
        }

        public DataItem.Builder<T> BeginDataItem<T>(bool isMultidimensional = false)
            where T : UIElement, new()
        {
            return new DataItem.Builder<T>(this, isMultidimensional);
        }

        public BlockItem.Builder<T> BeginBlockItem<T>()
            where T : UIElement, new()
        {
            return new BlockItem.Builder<T>(this);
        }

        public RowItem.Builder<T> BeginRowItem<T>()
            where T : UIElement, new()
        {
            return new RowItem.Builder<T>(this);
        }

        public SubviewItem.Builder<TView> BeginSubviewItem<TModel, TView>(TModel childModel, Action<TemplateBuilder, TModel> buildTemplateAction)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (buildTemplateAction == null)
                throw new ArgumentNullException(nameof(buildTemplateAction));

            return new SubviewItem.Builder<TView>(this, rowPresenter =>
            {
                if (rowPresenter.IsEof)
                    return null;
                return DataPresenter.Create(rowPresenter, childModel, buildTemplateAction);
            });
        }

        public SubviewItem.Builder<TView> BeginSubviewItem<TModel, TView>(_DataSet<TModel> child, Action<TemplateBuilder, TModel> buildTemplateAction)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (buildTemplateAction == null)
                throw new ArgumentNullException(nameof(buildTemplateAction));

            return new SubviewItem.Builder<TView>(this, rowPresenter =>
            {
                var dataRow = rowPresenter.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataPresenter.Create(childDataSet, buildTemplateAction);
            });
        }
    }
}
