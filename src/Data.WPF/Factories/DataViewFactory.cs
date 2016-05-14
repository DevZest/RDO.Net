using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataViewFactory
    {
        public static SubviewItem.Builder<DataView> DataView<T>(this TemplateBuilder templateBuilder, _DataSet<T> child,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return templateBuilder.SubviewItem<T, DataView>(child, builder)
                .Initialize(initializer);
        }

        public static SubviewItem.Builder<DataView> DataView<T>(this TemplateBuilder templateBuilder, T childModel,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return templateBuilder.SubviewItem<T, DataView>(childModel, builder)
                .Initialize(initializer);
        }
    }
}
