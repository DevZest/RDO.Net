using System;

namespace DevZest.Data.Wpf
{
    public static class ColumnExtensions
    {
        public static ColumnHeaderManager<TColumnHeader> Header<TColumnHeader>(this Column column, Action<TColumnHeader> initializer = null)
            where TColumnHeader : ColumnHeader, new()
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return new ColumnHeaderManager<TColumnHeader>(column, initializer);
        }
    }
}
