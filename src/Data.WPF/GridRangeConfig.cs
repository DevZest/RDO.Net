using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public struct GridRangeConfig
    {
        internal GridRangeConfig(DataSetPresenterConfig presenterConfig, GridRange gridRange)
        {
            PresenterConfig = presenterConfig;
            GridRange = gridRange;
        }

        public readonly DataSetPresenterConfig PresenterConfig;

        public readonly GridRange GridRange;

        private GridTemplate Template
        {
            get { return PresenterConfig.Presenter.Template; }
        }

        private void VerifyNotEmpty()
        {
            if (GridRange.IsEmpty)
                throw new InvalidOperationException(Strings.GridRange_VerifyNotEmpty);
        }

        public DataSetPresenterConfig Scalar<T>(Action<T> initializer, FlowMode flowMode = FlowMode.Repeat)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            var scalarItem = ScalarGridItem.Create(initializer, flowMode);
            Template.AddScalarItem(GridRange, scalarItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig List<T>(Action<DataRowPresenter, T> refresh, Func<T> constructor = null)
            where T : UIElement, new()
        {
            VerifyNotEmpty();
            if (refresh == null)
                throw new ArgumentNullException(nameof(refresh));

            var listItem = ListGridItem.Create(refresh, constructor);
            Template.AddListItem(GridRange, listItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig Child<T>(T childModel, Action<DataSetPresenterConfig, T> configAction, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            var childItem = new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(owner.DataRow.Children(childModel), configAction));
            Template.AddChildItem(GridRange, childItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig Child<T>(_DataSet<T> childDataSet, Action<DataSetPresenterConfig, T> configAction, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childDataSet == null)
                throw new ArgumentNullException(nameof(childDataSet));
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            var childItem = new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(childDataSet[owner.DataRow], configAction));
            Template.AddChildItem(GridRange, childItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig Repeat()
        {
            VerifyNotEmpty();

            Template.RepeatRange = GridRange;
            return PresenterConfig;
        }

        public DataSetPresenterConfig Repeat(GridOrientation value)
        {
            Repeat();
            Template.Orientation = value;
            return PresenterConfig;
        }
    }
}
