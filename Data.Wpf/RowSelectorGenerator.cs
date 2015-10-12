using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class RowSelectorGenerator : ViewGenerator
    {
        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.RowSelector; }
        }

        internal override UIElement CreateUIElement()
        {
            throw new NotImplementedException();
        }
    }
}
