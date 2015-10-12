using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ColumnHeaderGenerator : ViewGenerator
    {
        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.ColumnHeader; }
        }

        internal override UIElement CreateUIElement()
        {
            throw new NotImplementedException();
        }
    }
}
