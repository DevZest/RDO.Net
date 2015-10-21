using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ModelGridItem<T> : GridItem<T>
        where T : UIElement, new()
    {
        internal ModelGridItem(Model model, Action<T> initializer)
            : base(initializer)
        {
            Debug.Assert(model != null);
            Model = model;
        }

        public Model Model { get; private set; }

        internal override bool IsValidFor(Model model)
        {
            return Model == model;
        }
    }
}
