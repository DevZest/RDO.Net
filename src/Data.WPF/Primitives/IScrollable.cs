using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public interface  IScrollable
    {
        FrameworkElement Panel { get; }
        ContainerView CurrentContainerView { get; }
        ContainerViewList ContainerViewList { get; }
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }
        double ViewportWidth { get; }
        double ViewportHeight { get; }
        double ExtentWidth { get; }
        double ExtentHeight { get; }
        double HorizontalOffset { get; set; }
        double VerticalOffset { get; set; }
        int MaxGridExtentX { get; }
        int MaxGridExtentY { get; }
        double GetExtentX(int gridExtentX);
        double GetExtentY(int gridExtentY);
        double GetPositionX(int gridExtentX, GridPointPlacement placement);
        double GetPositionY(int gridExtentY, GridPointPlacement placement);
    }
}
