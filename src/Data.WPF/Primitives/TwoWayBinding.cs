using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class TwoWayBinding : Binding
    {
        internal abstract void FlushInput(UIElement element);

        internal abstract bool HasInputError { get; }
    }

    internal static class TwoWayBindingExtensions
    {
        internal static bool HasInputError(this IReadOnlyList<TwoWayBinding> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].HasInputError)
                    return true;
            }
            return false;
        }
    }
}
