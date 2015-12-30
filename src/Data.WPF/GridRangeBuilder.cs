using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct GridRangeBuilder
    {
        internal GridRangeBuilder(DataSetPresenterBuilder presenterBuilder, GridRange gridRange)
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

        public DataSetPresenterBuilder Scalar<T>(Action<T> initializer, FlowMode flowMode = FlowMode.Repeat)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            var scalarItem = ScalarGridItem.Create(initializer, flowMode);
            Template.AddScalarItem(GridRange, scalarItem);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder List<T>(Action<DataRowPresenter, T> refresh, Func<T> constructor = null)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (refresh == null)
                throw new ArgumentNullException(nameof(refresh));

            var listItem = ListGridItem.Create(refresh, constructor);
            Template.AddListItem(GridRange, listItem);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Child<T>(T childModel, Action<DataSetPresenterBuilder, T> builder, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var childItem = new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(owner.DataRow.Children(childModel), builder));
            Template.AddChildItem(GridRange, childItem);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Child<T>(_DataSet<T> childDataSet, Action<DataSetPresenterBuilder, T> builder, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childDataSet == null)
                throw new ArgumentNullException(nameof(childDataSet));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var childItem = new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(childDataSet[owner.DataRow], builder));
            Template.AddChildItem(GridRange, childItem);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Repeat()
        {
            VerifyNotEmpty();

            Template.RepeatRange = GridRange;
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Repeat(GridOrientation value)
        {
            Repeat();
            Template.Orientation = value;
            return PresenterBuilder;
        }
    }
}
