using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ModelViewGenerator<T> : ViewGenerator<T>
        where T : UIElement
    {
        internal ModelViewGenerator(Model model, Func<T> creator, Action<T> initializer)
            : base(creator, initializer)
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
