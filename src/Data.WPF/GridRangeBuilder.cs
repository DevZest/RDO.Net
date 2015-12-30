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

        public DataSetPresenterBuilder Scalar<T>(Action<T> initializer, FlowMode flowMode = FlowMode.Static, Action<T> cleanup = null, params IBehavior<T>[] behaviors)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            var scalarEntry = ScalarEntry.Create(initializer, flowMode, cleanup, behaviors);
            Template.AddScalarEntry(GridRange, scalarEntry);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder List<T>(Action<ListEntryBuilder<T>> builder)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var listEntry = ListEntry.Create<T>();
            using (var listEntryBuilder = new ListEntryBuilder<T>(listEntry))
            {
                builder(listEntryBuilder);
            }
            Template.AddListEntry(GridRange, listEntry);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Child<T>(T childModel, Action<DataSetPresenterBuilder, T> builder, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var childItem = new ChildEntry(viewConstructor, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                return DataSetPresenter.Create(dataRow.Children(childModel), builder);
            });
            Template.AddChildEntry(GridRange, childItem);
            return PresenterBuilder;
        }

        public DataSetPresenterBuilder Child<T>(_DataSet<T> child, Action<DataSetPresenterBuilder, T> builder, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var childItem = new ChildEntry(viewConstructor, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataSetPresenter.Create(childDataSet, builder);
            });
            Template.AddChildEntry(GridRange, childItem);
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
