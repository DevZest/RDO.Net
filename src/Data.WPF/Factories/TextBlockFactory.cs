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

            return rangeConfig.BeginListItem<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = x.GetSourceText(column).ToString())
                .End();
        }

        public static DataViewBuilder TextBlockLabel(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginListItem<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName + ":")
                .End();
        }

        public static DataViewBuilder TextBlockName(this GridRangeConfig rangeConfig, Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return rangeConfig.BeginListItem<TextBlock>()
                .Initialize(initializer)
                .Bind(x => x.Text = column.DisplayName)
                .End();
        }
    }
}
