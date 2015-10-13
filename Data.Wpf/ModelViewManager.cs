using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ModelViewManager<T> : ViewManager<T>
        where T : UIElement, new()
    {
        internal ModelViewManager(Model model, Action<T> initializer)
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
