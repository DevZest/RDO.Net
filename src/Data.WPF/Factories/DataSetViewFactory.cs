using System;

namespace DevZest.Data.Windows.Factories
{
    public static class DataSetViewFactory
    {
        public static DataSetPresenterBuilder DataSetView<T>(this DataSetPresenterBuilderRange builderRange, _DataSet<T> child,
            Action<DataSetPresenterBuilder, T> builder, Action<DataSetView> initializer = null)
            where T : Model, new()
        {
            return builderRange.BeginChildEntry<T, DataSetView>(child, builder)
                .Initialize(initializer)
                .End();
        }

        public static DataSetPresenterBuilder DataSetView<T>(this DataSetPresenterBuilderRange builderRange, T childModel,
            Action<DataSetPresenterBuilder, T> builder, Action<DataSetView> initializer = null)
            where T : Model, new()
        {
            return builderRange.BeginChildEntry<T, DataSetView>(childModel, builder)
                .Initialize(initializer)
                .End();
        }
    }
}
