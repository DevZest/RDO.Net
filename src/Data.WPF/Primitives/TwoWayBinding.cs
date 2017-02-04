using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        internal abstract void FlushInput(UIElement element);
    }
}
