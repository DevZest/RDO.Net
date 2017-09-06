using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        public abstract bool IsRefreshing { get; }

        internal abstract void FlushInput(UIElement element);
    }
}
