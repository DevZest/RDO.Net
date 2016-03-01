using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridRangeConfig
    {
        internal GridRangeConfig(DataPresenterBuilder presenterBuilder, GridRange gridRange)
        {
            _presenterBuilder = presenterBuilder;
            _gridRange = gridRange;
        }

        private readonly DataPresenterBuilder _presenterBuilder;

        private readonly GridRange _gridRange;

        int _autoSizeMeasureOrder;
        public GridRangeConfig AutoSizeMeasureOrder(int value)
        {
            _autoSizeMeasureOrder = value;
            return this;
        }

        private GridTemplate Template
        {
            get { return _presenterBuilder.Presenter.Template; }
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

        internal DataPresenterBuilder End(ScalarItem scalarItem)
        {
            scalarItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddScalarItem(_gridRange, scalarItem);
            return _presenterBuilder;
        }

        public RepeatItem.Builder<T> BeginRepeatItem<T>()
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            return new RepeatItem.Builder<T>(this);
        }

        internal DataPresenterBuilder End(RepeatItem repeatItem)
        {
            repeatItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddRepeatItem(_gridRange, repeatItem);
            return _presenterBuilder;
        }

        public ChildItem.Builder<TForm> BeginChildItem<TModel, TForm>(TModel childModel, Action<DataPresenterBuilder, TModel> builder)
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
                return DataPresenter.Create(owner, childModel, builder);
            });
        }

        public ChildItem.Builder<TForm> BeginChildItem<TModel, TForm>(_DataSet<TModel> child, Action<DataPresenterBuilder, TModel> builder)
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
                return DataPresenter.Create(childDataSet, builder);
            });
        }

        internal DataPresenterBuilder End(ChildItem childItem)
        {
            childItem.AutoSizeMeasureOrder = _autoSizeMeasureOrder;
            Template.AddChildItem(_gridRange, childItem);
            return _presenterBuilder;
        }

        public DataPresenterBuilder Repeat()
        {
            VerifyNotEmpty();

            Template.RepeatRange = _gridRange;
            return _presenterBuilder;
        }
    }
}
