using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnValueGenerator : ViewGenerator
    {
        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnValue; }
        }

        internal override UIElement CreateUIElement()
        {
            throw new NotImplementedException();
        }
    }
}
