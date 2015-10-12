using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnValueGenerator : ColumnViewGenerator<UIElement>
    {
        internal ColumnValueGenerator(Column column, Func<UIElement> creator, Action<UIElement> initializer)
            : base(column, creator, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnValue; }
        }
    }
}
