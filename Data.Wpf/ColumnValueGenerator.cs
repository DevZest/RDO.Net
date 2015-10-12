using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnValueGenerator<T> : ColumnViewGenerator<T>
        where T : UIElement, new()
    {
        internal ColumnValueGenerator(Column column, Action<T> initializer)
            : base(column, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnValue; }
        }
    }
}
