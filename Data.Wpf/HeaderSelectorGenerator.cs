
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class HeaderSelectorGenerator : ModelViewGenerator<HeaderSelector>
    {
        internal HeaderSelectorGenerator(Model model, Func<HeaderSelector> creator, Action<HeaderSelector> initializer)
            : base(model, creator, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.HeaderSelector; }
        }
    }
}
