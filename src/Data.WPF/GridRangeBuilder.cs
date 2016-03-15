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

        public ScalarItem.Builder<T> BeginScalarItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ScalarItem.Builder<T>(this);
        }

        internal TemplateBuilder End(ScalarItem scalarItem)
        {
            scalarItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddScalarItem(_gridRange, scalarItem);
            return _templateBuilder;
        }

        public RepeatItem.Builder<T> BeginRepeatItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new RepeatItem.Builder<T>(this);
        }

        internal TemplateBuilder End(RepeatItem repeatItem)
        {
            repeatItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddRepeatItem(_gridRange, repeatItem);
            return _templateBuilder;
        }

        public SubviewItem.Builder<TView> BeginSubviewItem<TModel, TView>(TModel childModel, Action<TemplateBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new SubviewItem.Builder<TView>(this, owner =>
            {
                if (owner.Kind != RowKind.DataRow)
                    return null;
                return DataPresenter.Create(owner, childModel, builder);
            });
        }

        public SubviewItem.Builder<TView> BeginSubviewItem<TModel, TView>(_DataSet<TModel> child, Action<TemplateBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new SubviewItem.Builder<TView>(this, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataPresenter.Create(childDataSet, builder);
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

            Template.RepeatRange = _gridRange;
            return _templateBuilder;
        }
    }
}
