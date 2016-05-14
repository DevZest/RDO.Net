using DevZest.Data.Windows.Primitives;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Factories
{
    public static class TextBlockFactory
    {
        public static RowItem.Builder<TextBlock> TextBlock(this TemplateBuilder templateBuilder, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return templateBuilder.RowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((row, x) => x.Text = row[column].GetText());
        }

        private static string GetText(this object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        public static RowItem.Builder<TextBlock> TextBlockLabel(this TemplateBuilder templateBuilder, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return templateBuilder.RowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName + ":");
        }

        public static RowItem.Builder<TextBlock> TextBlockName(this TemplateBuilder templateBuilder, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return templateBuilder.RowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName);
        }
    }
}
