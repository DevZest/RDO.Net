using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataViewFactory
    {
        public static TemplateBuilder DataView<T>(this GridRangeBuilder rangeConfig, _DataSet<T> child,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginSubviewItem<T, DataView>(child, builder)
                .Initialize(initializer)
                .End();
        }

        public static TemplateBuilder DataView<T>(this GridRangeBuilder rangeConfig, T childModel,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return rangeConfig.BeginSubviewItem<T, DataView>(childModel, builder)
                .Initialize(initializer)
                .End();
        }
    }
}
