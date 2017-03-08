using System.Windows;

namespace DevZest.Windows.Data.Primitives
{
    public interface  IScrollable
    {
        FrameworkElement Panel { get; }
        ContainerViewList ContainerViewList { get; }
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }
        double ViewportWidth { get; }
        double ViewportHeight { get; }
        double ScrollableWidth { get; }
        double ScrollableHeight { get; }
        double ExtentWidth { get; }
        double ExtentHeight { get; }
        double HorizontalOffset { get; }
        double VerticalOffset { get; }
        int MaxGridExtentX { get; }
        int MaxGridExtentY { get; }
        int FrozenHeadGridExtentX { get; }
        int FrozenTailGridExtentX { get; }
        int FrozenHeadGridExtentY { get; }
        int FrozenTailGridExtentY { get; }
        double GetExtentX(int gridExtentX);
        double GetExtentY(int gridExtentY);
        double GetPositionX(int gridExtentX, GridPlacement placement);
        double GetPositionY(int gridExtentY, GridPlacement placement);
        void ScrollToX(int gridExtent, double fraction, GridPlacement placement);
        void ScrollToY(int gridExtent, double fraction, GridPlacement placement);
        void ScrollBy(double x, double y);
        void ScrollTo(double x, double y);
        void EnsureCurrentRowVisible();
        int GetPageHeadContainerOrdinal(bool enforceCurrent);
        int GetPageTailContainerOrdinal(bool enforceCurrent);
    }
}
