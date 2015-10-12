using System;

namespace DevZest.Data.Wpf
{
    public static class DataSetControlInitialization
    {
        public static void Initialize<TDataSetControl, TModel>(this TDataSetControl dataSetControl, DataSet<TModel> dataSet, Action<DataSetControl, TModel> initializer = null)
            where TDataSetControl : DataSetControl
            where TModel : Model, new()
        {
            if (dataSetControl == null)
                throw new ArgumentNullException(nameof(dataSetControl));
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            dataSetControl.BeginInitialization(null, DataSet.Get(dataSet));
            if (initializer != null)
                initializer(dataSetControl, dataSet._);
            else
                dataSetControl.DefaultInitialize();
        }

        public static T RowOrientationX<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.RowOrientation = RowOrientation.X;
            return dataSetControl;
        }

        public static T RowOrientationY<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.RowOrientation = RowOrientation.Y;
            return dataSetControl;
        }

        public static T RowOrientationZ<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.RowOrientation = RowOrientation.Z;
            return dataSetControl;
        }

        public static T RowOrientationXY<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.RowOrientation = RowOrientation.XY;
            return dataSetControl;
        }

        public static T RowOrientationYX<T>(this T dataSetControl)
            where T : DataSetControl
        {
            dataSetControl.RowOrientation = RowOrientation.YX;
            return dataSetControl;
        }

        public static T Panel<T, TChild>(this T dataSetControl, GridRange gridRange, PanelGenerator<TChild> generator)
            where T : DataSetControl
            where TChild : DataSetControl, new()
        {
            dataSetControl.InitPanel(gridRange, generator);
            return dataSetControl;
        }
    }
}
