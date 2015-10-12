using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnHeaderGenerator<T> : ColumnViewGenerator<T>
        where T : ColumnHeader, new()
    {
        internal ColumnHeaderGenerator(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnHeader; }
        }
    }
}
