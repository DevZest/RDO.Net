using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views.Primitives
{
    public interface ICompositeView
    {
        ContentPresenter GetPlaceholder(string name);
        CompositeBinding CompositeBinding { get; }
        IReadOnlyList<UIElement> Children { get; }
    }
}
