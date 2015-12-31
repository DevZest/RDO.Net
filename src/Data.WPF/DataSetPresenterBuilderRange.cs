using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct DataSetPresenterBuilderRange
    {
        internal DataSetPresenterBuilderRange(DataSetPresenterBuilder presenterBuilder, GridRange gridRange)
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

        public ScalarEntryBuilder<T> BeginScalarEntry<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ScalarEntryBuilder<T>(this);
        }

        internal DataSetPresenterBuilder ScalarEntry(ScalarEntry scalarEntry)
        {
            Template.AddScalarEntry(GridRange, scalarEntry);
            return PresenterBuilder;
        }

        public ListEntryBuilder<T> BeginListEntry<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new ListEntryBuilder<T>(this);
        }

        internal DataSetPresenterBuilder ListEntry(ListEntry listEntry)
        {
            Template.AddListEntry(GridRange, listEntry);
            return PresenterBuilder;
        }

        public ChildEntryBuilder<TView> BeginChildEntry<TModel, TView>(TModel childModel, Action<DataSetPresenterBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataSetView, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildEntryBuilder<TView>(this, owner =>
            {
                var dataRow = owner.DataRow;
                if (dataRow == null)
                    return null;
                return DataSetPresenter.Create(dataRow.Children(childModel), builder);
            });
        }

        public ChildEntryBuilder<TView> BeginChildEntry<TModel, TView>(_DataSet<TModel> child, Action<DataSetPresenterBuilder, TModel> builder)
            where TModel : Model, new()
            where TView : DataSetView, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new ChildEntryBuilder<TView>(this, owner =>
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

        internal DataSetPresenterBuilder ChildEntry(ChildEntry childEntry)
        {
            Template.AddChildEntry(GridRange, childEntry);
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
