using System.Windows;

namespace DevZest.Windows.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        internal abstract void FlushInput(UIElement element);
    }
}
