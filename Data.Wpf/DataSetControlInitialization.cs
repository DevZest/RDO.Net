using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public static class DataSetControlInitialization
    {
        public static void Initialize<TDataSetControl, TModel>(this TDataSetControl dataSetControl, DataSet<TModel> dataSet, Action<TDataSetControl, TModel> initializer = null)
            where TDataSetControl : DataSetControl
            where TModel : Model, new()
        {
            if (dataSetControl == null)
                throw new ArgumentNullException(nameof(dataSetControl));
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var dataSetView = dataSetControl.View;
            dataSetView.BeginInitialization(DataSet.Get(dataSet));
            if (initializer != null)
                initializer(dataSetControl, dataSet._);
            else
                dataSetControl.DefaultInitialize();
            dataSetView.EndInitialization();
        }

        public static T Orientation<T>(this T dataSetControl, LayoutOrientation orientation)
            where T : DataSetControl
        {
            dataSetControl.Orientation = orientation;
            return dataSetControl;
        }

        public static T Scrollable<T>(this T dataSetControl, bool value)
            where T : DataSetControl
        {
            dataSetControl.Scrollable = value;
            return dataSetControl;
        }

        public static T GridRows<T>(this T dataSetControl, params string[] heights)
            where T : DataSetControl
        {
            if (heights != null)
                throw new ArgumentNullException(nameof(heights));

            var dataSetView = dataSetControl.View;
            foreach (var height in heights)
                dataSetView.InitGridRow(height);
            return dataSetControl;
        }

        public static T GridColumns<T>(this T dataSetControl, params string[] widths)
            where T : DataSetControl
        {
            if (widths != null)
                throw new ArgumentNullException(nameof(widths));

            var dataSetView = dataSetControl.View;
            foreach (var width in widths)
                dataSetView.InitGridColumn(width);
            return dataSetControl;
        }

        public static T RowsPanel<T>(this T dataSetControl, GridRange value)
            where T : DataSetControl
        {
            dataSetControl.View.RowsPanelRange = value;
            return dataSetControl;
        }

        public static T ChildSet<T, TChild>(this T dataSetControl, GridRange gridRange, ChildSetViewItem<TChild> viewItem)
            where T : DataSetControl
            where TChild : DataSetControl, new()
        {
            dataSetControl.View.InitViewItem(gridRange, viewItem);
            return dataSetControl;
        }

        public static T ColumnValue<T, TUIElement>(this T dataSetControl, GridRange gridRange, ColumnValueViewItem<TUIElement> viewItem)
            where T : DataSetControl
            where TUIElement : UIElement, new()
        {
            dataSetControl.View.InitViewItem(gridRange, viewItem);
            return dataSetControl;
        }

        public static T HeaderSelector<T>(this T dataSetControl, GridRange gridRange, Action<SetSelector> initializer = null)
            where T : DataSetControl
        {
            dataSetControl.View.InitViewItem(gridRange, new HeaderSelectorViewItem<SetSelector>(dataSetControl.Model, initializer));
            return dataSetControl;
        }

        public static T RowSelector<T>(this T dataSetControl, GridRange gridRange, Action<RowSelector> initializer = null)
            where T : DataSetControl
        {
            dataSetControl.View.InitViewItem(gridRange, new DataRowSelectorViewItem<RowSelector>(dataSetControl.Model, initializer));
            return dataSetControl;
        }

        public static T ColumnHeader<T>(this T dataSetControl, GridRange gridRange, Column column, Action<ColumnHeader> initializer = null)
            where T : DataSetControl
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            dataSetControl.View.InitViewItem(gridRange, new ColumnHeaderViewItem<ColumnHeader>(column, initializer));
            return dataSetControl;
        }
    }
}
