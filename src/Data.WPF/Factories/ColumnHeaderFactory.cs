using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static TemplateBuilder ColumnHeader(this GridRangeBuilder rangeConfig, Column column, Action<ColumnHeader> initializer = null)
        {
            return rangeConfig.BeginRowItem<ColumnHeader>()
                .Initialize(initializer)
                .End();
        }
    }
}
