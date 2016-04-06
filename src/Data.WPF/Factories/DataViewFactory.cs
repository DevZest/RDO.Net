using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataViewFactory
    {
        public static TemplateBuilder DataView<T>(this TemplateItemBuilderFactory builderFactory, _DataSet<T> child,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return builderFactory.BeginSubviewItem<T, DataView>(child, builder)
                .Initialize(initializer)
                .End();
        }

        public static TemplateBuilder DataView<T>(this TemplateItemBuilderFactory builderFactory, T childModel,
            Action<TemplateBuilder, T> builder, Action<DataView> initializer = null)
            where T : Model, new()
        {
            return builderFactory.BeginSubviewItem<T, DataView>(childModel, builder)
                .Initialize(initializer)
                .End();
        }
    }
}
