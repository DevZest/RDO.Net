using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataFormFactory
    {
        public static DataViewBuilder DataForm<T>(this GridRangeConfig rangeConfig, _DataSet<T> child,
            Action<DataViewBuilder, T> builder, Action<DataForm> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginChildItem<T, DataForm>(child, builder)
                .Initialize(initializer)
                .End();
        }

        public static DataViewBuilder DataForm<T>(this GridRangeConfig rangeConfig, T childModel,
            Action<DataViewBuilder, T> builder, Action<DataForm> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginChildItem<T, DataForm>(childModel, builder)
                .Initialize(initializer)
                .End();
        }
    }
}
