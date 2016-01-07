using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static DataSetPresenterBuilder ColumnHeader(this DataSetPresenterBuilderRange builderRange, Column column, Action<ColumnHeader> initializer = null)
        {
            return builderRange.BeginRowEntry<ColumnHeader>()
                .Initialize(initializer)
                .End();
        }
    }
}
