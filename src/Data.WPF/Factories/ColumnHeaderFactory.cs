using System;

namespace DevZest.Data.Windows.Factories
{
    public static class ColumnHeaderFactory
    {
        public static TemplateBuilder ColumnHeader(this TemplateItemBuilderFactory builderFactory, Column column, Action<ColumnHeader> initializer = null)
        {
            return builderFactory.BeginDataItem<ColumnHeader>()
                .Initialize(initializer)
                .End();
        }
    }
}
