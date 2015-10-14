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

            dataSetControl.BeginInitialization(DataSet.Get(dataSet));
            if (initializer != null)
                initializer(dataSetControl, dataSet._);
            else
                dataSetControl.DefaultInitialize();
            dataSetControl.EndInitialization();
        }

        public static T OrientationX<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.Orientation = LayoutOrientation.X;
            return dataSetControl;
        }

        public static T OrientationY<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.Orientation = LayoutOrientation.Y;
            return dataSetControl;
        }

        public static T OrientationZ<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.Orientation = LayoutOrientation.Z;
            return dataSetControl;
        }

        public static T OrientationXY<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.Orientation = LayoutOrientation.XY;
            return dataSetControl;
        }

        public static T OrientationYX<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.Orientation = LayoutOrientation.YX;
            return dataSetControl;
        }

        public static T GridRows<T>(this T dataSetControl, params string[] heights)
            where T : DataSetControl
        {
            if (heights != null)
                throw new ArgumentNullException(nameof(heights));

            foreach (var height in heights)
                dataSetControl.InitGridRow(height);
            return dataSetControl;
        }

        public static T GridColumns<T>(this T dataSetControl, params string[] widths)
            where T : DataSetControl
        {
            if (widths != null)
                throw new ArgumentNullException(nameof(widths));

            foreach (var width in widths)
                dataSetControl.InitGridColumn(width);
            return dataSetControl;
        }

        public static T RowsPanel<T>(this T dataSetControl, GridRange value)
            where T : DataSetControl
        {
            dataSetControl.RowsPanelRange = value;
            return dataSetControl;
        }

        public static T ChildSet<T, TChild>(this T dataSetControl, GridRange gridRange, ChildSetManager<TChild> manager)
            where T : DataSetControl
            where TChild : DataSetControl, new()
        {
            dataSetControl.InitView(gridRange, manager);
            return dataSetControl;
        }

        public static T ColumnValue<T, TUIElement>(this T dataSetControl, GridRange gridRange, ColumnValueManager<TUIElement> manager)
            where T : DataSetControl
            where TUIElement : UIElement, new()
        {
            dataSetControl.InitView(gridRange, manager);
            return dataSetControl;
        }

        public static T HeaderSelector<T>(this T dataSetControl, GridRange gridRange, Action<SetSelector> initializer = null)
            where T : DataSetControl
        {
            dataSetControl.InitView(gridRange, new HeaderSelectorManager<SetSelector>(dataSetControl.Model, initializer));
            return dataSetControl;
        }

        public static T RowSelector<T>(this T dataSetControl, GridRange gridRange, Action<RowSelector> initializer = null)
            where T : DataSetControl
        {
            dataSetControl.InitView(gridRange, new DataRowSelectorManager<RowSelector>(dataSetControl.Model, initializer));
            return dataSetControl;
        }


        public static T ColumnHeader<T>(this T dataSetControl, GridRange gridRange, Column column, Action<ColumnHeader> initializer = null)
            where T : DataSetControl
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            dataSetControl.InitView(gridRange, new ColumnHeaderManager<ColumnHeader>(column, initializer));
            return dataSetControl;
        }
    }
}
