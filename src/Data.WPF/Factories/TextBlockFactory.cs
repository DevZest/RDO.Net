using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Factories
{
    public static class TextBlockFactory
    {
        public static DataViewBuilder TextBlock(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRepeatItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = src.RowView.GetValue(column).GetText())
                .End();
        }

        private static string GetText(this object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        public static DataViewBuilder TextBlockLabel(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRepeatItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName + ":")
                .End();
        }

        public static DataViewBuilder TextBlockName(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginRepeatItem<TextBlock>()
                .Initialize(initializer)
                .Bind((src, x) => x.Text = column.DisplayName)
                .End();
        }
    }
}
