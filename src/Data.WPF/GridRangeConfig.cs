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

        public ScalarUnit.Builder<T> BeginScalarUnit<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ScalarUnit.Builder<T>(this);
        }

        internal DataViewBuilder End(ScalarUnit scalarUnit)
        {
            scalarUnit.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddScalarUnit(_gridRange, scalarUnit);
            return _viewBuilder;
        }

        public ListUnit.Builder<T> BeginListUnit<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ListUnit.Builder<T>(this);
        }

        internal DataViewBuilder End(ListUnit listUnit)
        {
            listUnit.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddListUnit(_gridRange, listUnit);
            return _viewBuilder;
        }

        public ChildUnit.Builder<TForm> BeginChildUnit<TModel, TForm>(TModel childModel, Action<DataViewBuilder, TModel> builder)
            where TModel : Model, new()
            where TForm : DataForm, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildUnit.Builder<TForm>(this, owner =>
            {
                if (owner.Kind != RowKind.DataRow)
                    return null;
                return DataView.Create(owner, childModel, builder);
            });
        }

        public ChildUnit.Builder<TForm> BeginChildUnit<TModel, TForm>(_DataSet<TModel> child, Action<DataViewBuilder, TModel> builder)
            where TModel : Model, new()
            where TForm : DataForm, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildUnit.Builder<TForm>(this, owner =>
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

        internal DataViewBuilder End(ChildUnit childUnit)
        {
            childUnit.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddChildUnit(_gridRange, childUnit);
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
