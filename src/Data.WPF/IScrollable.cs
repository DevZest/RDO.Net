using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public interface  IScrollable
    {
        ContainerView CurrentContainerView { get; }
        IReadOnlyList<ContainerView> ContainerViewList { get; }
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }
    }
}
