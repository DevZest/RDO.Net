using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridRangeConfig
    {
        internal GridRangeConfig(DataViewBuilder viewBuilder, GridRange gridRange)
        {
            _viewBuilder = viewBuilder;
            _gridRange = gridRange;
        }

        private readonly DataViewBuilder _viewBuilder;

        private readonly GridRange _gridRange;

        int _autoSizeMeasureOrder;
        public GridRangeConfig AutoSizeMeasureOrder(int value)
        {
            _autoSizeMeasureOrder = value;
            return this;
        }

        private GridTemplate Template
        {
            get { return _viewBuilder.View.Template; }
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

        internal DataViewBuilder End(ScalarItem scalarItem)
        {
            scalarItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddScalarItem(_gridRange, scalarItem);
            return _viewBuilder;
        }

        public ListItem.Builder<T> BeginListItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ListItem.Builder<T>(this);
        }

        internal DataViewBuilder End(ListItem listItem)
        {
            listItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddListItem(_gridRange, listItem);
            return _viewBuilder;
        }

        public ChildItem.Builder<TForm> BeginChildItem<TModel, TForm>(TModel childModel, Action<DataViewBuilder, TModel> builder)
            where TModel : Model, new()
            where TForm : DataForm, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildItem.Builder<TForm>(this, owner =>
            {
                if (owner.Kind != RowKind.DataRow)
                    return null;
                return DataView.Create(owner, childModel, builder);
            });
        }

        public ChildItem.Builder<TForm> BeginChildItem<TModel, TForm>(_DataSet<TModel> child, Action<DataViewBuilder, TModel> builder)
            where TModel : Model, new()
            where TForm : DataForm, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildItem.Builder<TForm>(this, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataView.Create(childDataSet, builder);
            });
        }

        internal DataViewBuilder End(ChildItem childItem)
        {
            childItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddChildItem(_gridRange, childItem);
            return _viewBuilder;
        }

        public DataViewBuilder AsListRange()
        {
            VerifyNotEmpty();

            Template.ListRange = _gridRange;
            return _viewBuilder;
        }
    }
}
