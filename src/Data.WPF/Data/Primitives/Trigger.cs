using System;
using System.Windows;

namespace DevZest.Windows.Data.Primitives
{
    public abstract class Trigger<T>
        where T : UIElement, new()
    {
        protected Trigger()
        {
        }

        internal Action<T> ExecuteAction { get; set; }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected void Execute(T element)
        {
            ExecuteAction(element);
        }
    }
}
