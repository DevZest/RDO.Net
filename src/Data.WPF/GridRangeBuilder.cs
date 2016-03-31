using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridRangeBuilder
    {
        internal GridRangeBuilder(TemplateBuilder templateBuilder, GridRange gridRange)
        {
            _templateBuilder = templateBuilder;
            _gridRange = gridRange;
        }

        private readonly TemplateBuilder _templateBuilder;

        private readonly GridRange _gridRange;

        int _autoSizeMeasureOrder;
        public GridRangeBuilder AutoSizeMeasureOrder(int value)
        {
            _autoSizeMeasureOrder = value;
            return this;
        }

        private Template Template
        {
            get { return _templateBuilder.Template; }
        }

        private void VerifyNotEmpty()
        {
            if (_gridRange.IsEmpty)
                throw new InvalidOperationException(Strings.GridRange_VerifyNotEmpty);
        }

        public DataItem.Builder<T> BeginDataItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new DataItem.Builder<T>(this);
        }

        internal TemplateBuilder End(DataItem dataItem)
        {
            dataItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddDataItem(_gridRange, dataItem);
            return _templateBuilder;
        }

        public RowItem.Builder<T> BeginRowItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new RowItem.Builder<T>(this);
        }

        internal TemplateBuilder End(RowItem rowItem)
        {
            rowItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddRowItem(_gridRange, rowItem);
            return _templateBuilder;
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

        internal TemplateBuilder End(SubviewItem subviewItem)
        {
            subviewItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddSubviewItem(_gridRange, subviewItem);
            return _templateBuilder;
        }

        public TemplateBuilder Repeat()
        {
            VerifyNotEmpty();

            Template.RowRange = _gridRange;
            return _templateBuilder;
        }
    }
}
