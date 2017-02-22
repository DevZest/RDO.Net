using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    public interface  IScrollable
    {
        ContainerView CurrentContainerView { get; }
        IReadOnlyList<ContainerView> ContainerViewList { get; }
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }
    }
}
