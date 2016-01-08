using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct GridRangeConfig
    {
        internal GridRangeConfig(DataSetPresenterBuilder presenterBuilder, GridRange gridRange)
        {
            PresenterBuilder = presenterBuilder;
            GridRange = gridRange;
        }

        public readonly DataSetPresenterBuilder PresenterBuilder;

        public readonly GridRange GridRange;

        private GridTemplate Template
        {
            get { return PresenterBuilder.Presenter.Template; }
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

        internal DataSetPresenterBuilder End(ScalarUnit scalarUnit)
        {
            Template.AddScalarUnit(GridRange, scalarUnit);
            return PresenterBuilder;
        }

        public ListUnit.Builder<T> BeginListUnit<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ListUnit.Builder<T>(this);
        }

        internal DataSetPresenterBuilder End(ListUnit listUnit)
        {
            Template.AddListUnit(GridRange, listUnit);
            return PresenterBuilder;
        }

        public ChildUnit.Builder<TView> BeginChildUnit<TModel, TView>(TModel childModel, Action<DataSetPresenterBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataSetView, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildUnit.Builder<TView>(this, owner =>
            {
                if (owner.RowType != RowType.DataRow)
                    return null;
                return DataSetPresenter.Create(owner, childModel, builder);
            });
        }

        public ChildUnit.Builder<TView> BeginChildUnit<TModel, TView>(_DataSet<TModel> child, Action<DataSetPresenterBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataSetView, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildUnit.Builder<TView>(this, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataSetPresenter.Create(childDataSet, builder);
            });
        }

        internal DataSetPresenterBuilder End(ChildUnit childUnit)
        {
            Template.AddChildUnit(GridRange, childUnit);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder AsListRange()
        {
            VerifyNotEmpty();

            Template.ListRange = GridRange;
            return PresenterBuilder;
        }
    }
}
