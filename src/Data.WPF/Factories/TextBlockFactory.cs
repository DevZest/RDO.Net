using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Factories
{
    public static class TextBlockFactory
    {
        public static TemplateBuilder TextBlock(this GridRangeBuilder rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((row, x) => x.Text = row[column].GetText())
                .End();
        }

        private static string GetText(this object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        public static TemplateBuilder TextBlockLabel(this GridRangeBuilder rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName + ":")
                .End();
        }

        public static TemplateBuilder TextBlockName(this GridRangeBuilder rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRowItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName)
                .End();
        }
    }
}
