using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ExplicitInput<T> : Input<T>
        where T : UIElement, new()
    {
        public ExplicitInput()
        {
        }

        protected internal override void Attach(T element)
        {
        }

        protected internal override void Detach(T element)
        {
        }
    }
}
