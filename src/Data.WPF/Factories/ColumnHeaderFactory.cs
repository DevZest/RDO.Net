using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static ScalarItem.Builder<ColumnHeader> ColumnHeader(this TemplateBuilder templateBuilder, Column column, Action<ColumnHeader> initializer = null)
        {
            return templateBuilder.ScalarItem<ColumnHeader>()
                .Initialize(initializer);
        }
    }
}
