using DevZest.Data.Views.Primitives;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal interface ICompositeBinding
    {
        IReadOnlyList<Binding> Bindings { get; }
        IReadOnlyList<string> Names { get; }
        void Setup<T>(T compositeView) where T : UIElement, ICompositeView;
    }
}
