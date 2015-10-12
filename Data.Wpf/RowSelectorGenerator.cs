using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class RowSelectorGenerator<T> : ModelViewGenerator<T>
        where T : RowSelector, new()
    {
        internal RowSelectorGenerator(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.RowSelector; }
        }
    }
}
