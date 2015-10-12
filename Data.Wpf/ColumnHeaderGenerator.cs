using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnHeaderGenerator : ColumnViewGenerator<ColumnHeader>
    {
        internal ColumnHeaderGenerator(Column column, Func<ColumnHeader> creator, Action<ColumnHeader> initializer)
            : base(column, creator, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnHeader; }
        }
    }
}
