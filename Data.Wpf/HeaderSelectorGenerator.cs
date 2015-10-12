
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class HeaderSelectorGenerator : ViewGenerator
    {
        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.HeaderSelector; }
        }

        internal override UIElement CreateUIElement()
        {
            throw new NotImplementedException();
        }
    }
}
