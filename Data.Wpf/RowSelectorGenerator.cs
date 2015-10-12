using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class RowSelectorGenerator : ModelViewGenerator<RowSelector>
    {
        internal RowSelectorGenerator(Model model, Func<RowSelector> creator, Action<RowSelector> initializer)
            : base(model, creator, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.RowSelector; }
        }
    }
}
