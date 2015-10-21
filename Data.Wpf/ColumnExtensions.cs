using System;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public static class ColumnExtensions
    {
        public static ColumnValueGridItem<TextBlock> TextBlock(this Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ColumnValueGridItem<TextBlock>(column, initializer);
        }

        public static ColumnValueGridItem<TTextBlock> TextBlock<TTextBlock>(this Column column, Action<TTextBlock> initializer)
            where TTextBlock : TextBlock, new()
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ColumnValueGridItem<TTextBlock>(column, initializer);
        }
    }
}
