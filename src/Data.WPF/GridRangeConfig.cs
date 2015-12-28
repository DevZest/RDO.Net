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

        private void VerifyGridItem(GridItem gridItem, string paramGridItemName)
        {
            VerifyNotEmpty();
            if (gridItem == null)
                throw new ArgumentNullException(paramGridItemName);
        }

        public DataSetPresenterConfig GridItem<T>(ScalarGridItem<T> scalarItem)
            where T : UIElement, new()
        {
            VerifyGridItem(scalarItem, nameof(scalarItem));
            Template.AddItem(GridRange, scalarItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig GridItem<T>(ListGridItem<T> listItem)
            where T : UIElement, new()
        {
            VerifyGridItem(listItem, nameof(listItem));
            Template.AddItem(GridRange, listItem);
            return PresenterConfig;
        }

        public DataSetPresenterConfig GridItem(ChildGridItem childItem)
        {
            VerifyGridItem(childItem, nameof(childItem));
            Template.AddItem(GridRange, childItem);
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
