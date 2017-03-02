using System.Windows;

namespace DevZest.Windows.Data.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        internal abstract void FlushInput(UIElement element);
    }
}
