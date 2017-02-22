using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public interface  IScrollable
    {
        FrameworkElement Panel { get; }
        ContainerView CurrentContainerView { get; }
        ContainerViewList ContainerViewList { get; }
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }
    }
}
