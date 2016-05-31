using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows.Factories
{
    public static class RowHeaderFactory
    {
        public static RowItem.Builder<RowHeader> RowHeader(this TemplateBuilder templateBuilder, Action<RowHeader> initializer = null)
        {
            return templateBuilder.RowItem<RowHeader>()
                .Initialize(initializer);
        }
    }
}
