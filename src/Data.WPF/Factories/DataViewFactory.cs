using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataViewFactory
    {
        public static DataPresenterBuilder DataView<T>(this GridRangeConfig rangeConfig, _DataSet<T> child,
            Action<DataPresenterBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginSubviewItem<T, DataView>(child, builder)
                .Initialize(initializer)
                .End();
        }

        public static DataPresenterBuilder DataView<T>(this GridRangeConfig rangeConfig, T childModel,
            Action<DataPresenterBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginSubviewItem<T, DataView>(childModel, builder)
                .Initialize(initializer)
                .End();
        }
    }
}
