using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static DataPresenterBuilder ColumnHeader(this GridRangeConfig rangeConfig, Column column, Action<ColumnHeader> initializer = null)
        {
            return rangeConfig.BeginRepeatItem<ColumnHeader>()
                .Initialize(initializer)
                .End();
        }
    }
}
