using System;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public static class ColumnExtensions
    {
        public static ColumnValueManager<TextBlock> TextBlock(this Column column, Action<TextBlock> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ColumnValueManager<TextBlock>(column, initializer);
        }

        public static ColumnValueManager<TTextBlock> TextBlock<TTextBlock>(this Column column, Action<TTextBlock> initializer)
            where TTextBlock : TextBlock, new()
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ColumnValueManager<TTextBlock>(column, initializer);
        }
    }
}
