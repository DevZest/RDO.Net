using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        internal abstract void FlushInput(UIElement element);
    }
}
