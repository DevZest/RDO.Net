
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class HeaderSelectorGenerator<T> : ModelViewGenerator<T>
        where T : HeaderSelector, new()
    {
        internal HeaderSelectorGenerator(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.HeaderSelector; }
        }
    }
}
