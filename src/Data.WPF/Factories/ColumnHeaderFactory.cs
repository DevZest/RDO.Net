using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static DataViewBuilder ColumnHeader(this GridRangeConfig rangeConfig, Column column, Action<ColumnHeader> initializer = null)
        {
            return rangeConfig.BeginListUnit<ColumnHeader>()
                .Initialize(initializer)
                .End();
        }
    }
}
