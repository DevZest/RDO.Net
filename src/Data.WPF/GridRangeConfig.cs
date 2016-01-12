using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct GridRangeConfig
    {
        internal GridRangeConfig(DataViewBuilder viewBuilder, GridRange gridRange)
        {
            ViewBuilder = viewBuilder;
            GridRange = gridRange;
        }

        internal readonly DataViewBuilder ViewBuilder;

        internal readonly GridRange GridRange;

        private GridTemplate Template
        {
            get { return ViewBuilder.View.Template; }
        }

        private void VerifyNotEmpty()
        {
            if (GridRange.IsEmpty)
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
            Template.AddScalarUnit(GridRange, scalarUnit);
            return ViewBuilder;
        }

        public ListUnit.Builder<T> BeginListUnit<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ListUnit.Builder<T>(this);
        }

        internal DataViewBuilder End(ListUnit listUnit)
        {
            Template.AddListUnit(GridRange, listUnit);
            return ViewBuilder;
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
            Template.AddChildUnit(GridRange, childUnit);
            return ViewBuilder;
        }

        public DataViewBuilder AsListRange()
        {
            VerifyNotEmpty();

            Template.ListRange = GridRange;
            return ViewBuilder;
        }
    }
}
