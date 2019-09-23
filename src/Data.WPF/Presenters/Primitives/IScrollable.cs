using DevZest.Data.Views.Primitives;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Contains layout scrolling logic.
    /// </summary>
    public interface  IScrollable
    {
        /// <summary>
        /// Gets the panel that is the container of view elements.
        /// </summary>
        FrameworkElement Panel { get; }

        /// <summary>
        /// Gets the list of <see cref="ContainerView"/> objects.
        /// </summary>
        ContainerViewList ContainerViewList { get; }

        /// <summary>
        /// Gets the placement of current <see cref="ContainerView"/>.
        /// </summary>
        CurrentContainerViewPlacement CurrentContainerViewPlacement { get; }

        /// <summary>
        /// Gets the width of the viewport.
        /// </summary>
        double ViewportWidth { get; }

        /// <summary>
        /// Gets the height of the viewport.
        /// </summary>
        double ViewportHeight { get; }

        /// <summary>
        /// Gets the scrollable width which is view port width minus width of frozen grid column(s).
        /// </summary>
        double ScrollableWidth { get; }

        /// <summary>
        /// Gets the scrollable height which is view port height minus height of frozen grid row(s).
        /// </summary>
        double ScrollableHeight { get; }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        double ExtentWidth { get; }

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        double ExtentHeight { get; }

        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        double HorizontalOffset { get; }

        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        double VerticalOffset { get; }

        /// <summary>
        /// Gets the max extent of grid columns.
        /// </summary>
        int MaxGridExtentX { get; }

        /// <summary>
        /// Gets the max extent of grid rows.
        /// </summary>
        int MaxGridExtentY { get; }

        /// <summary>
        /// Gets the extent of grid column for frozen header.
        /// </summary>
        int FrozenHeadGridExtentX { get; }

        /// <summary>
        /// Gets the extent of grid column for frozen tail.
        /// </summary>
        int FrozenTailGridExtentX { get; }

        /// <summary>
        /// Gets the extent of grid row for frozen header.
        /// </summary>
        int FrozenHeadGridExtentY { get; }

        /// <summary>
        /// Gets the extent of grid row for frozen tail.
        /// </summary>
        int FrozenTailGridExtentY { get; }

        /// <summary>
        /// Gets the horizontal extent position for specified grid extent value.
        /// </summary>
        /// <param name="gridExtentX">The grid extent value.</param>
        /// <returns>The position.</returns>
        double GetExtentX(int gridExtentX);

        /// <summary>
        /// Gets the vertical extent position for specified grid extent value.
        /// </summary>
        /// <param name="gridExtentY">The grid extent value.</param>
        /// <returns>The position.</returns>
        double GetExtentY(int gridExtentY);

        /// <summary>
        /// Gets horizontal position from grid extent.
        /// </summary>
        /// <param name="gridExtentX">The grid extent.</param>
        /// <param name="placement">The grid placement.</param>
        /// <returns>The position.</returns>
        double GetPositionX(int gridExtentX, GridPlacement placement);

        /// <summary>
        /// Gets vertial position from grid extent.
        /// </summary>
        /// <param name="gridExtentY">The grid extent.</param>
        /// <param name="placement">The grid placement.</param>
        /// <returns>The position.</returns>
        double GetPositionY(int gridExtentY, GridPlacement placement);

        /// <summary>
        /// Scrolls to horizontal grid extent position.
        /// </summary>
        /// <param name="gridExtent">The grid extent.</param>
        /// <param name="fraction">Fraction within the grid extent.</param>
        /// <param name="placement">The grid placement.</param>
        void ScrollToX(int gridExtent, double fraction, GridPlacement placement);

        /// <summary>
        /// Scrolls to vertical grid extent position.
        /// </summary>
        /// <param name="gridExtent">The grid extent.</param>
        /// <param name="fraction">Fraction within the grid extent.</param>
        /// <param name="placement">The grid placement.</param>
        void ScrollToY(int gridExtent, double fraction, GridPlacement placement);

        /// <summary>
        /// Scrolls specified offset.
        /// </summary>
        /// <param name="x">The horizontal offset.</param>
        /// <param name="y">The vertical offset.</param>
        void ScrollBy(double x, double y);

        /// <summary>
        /// Scrolls to specified offset.
        /// </summary>
        /// <param name="x">The horizontal offset.</param>
        /// <param name="y">The vertical offset.</param>
        void ScrollTo(double x, double y);

        /// <summary>
        /// Ensures current row is visible.
        /// </summary>
        void EnsureCurrentRowVisible();

        /// <summary>
        /// Scrolls one page up.
        /// </summary>
        void ScrollPageUp();

        /// <summary>
        /// Scrolls one page down.
        /// </summary>
        void ScrollPageDown();

        /// <summary>
        /// Scrolls to row at page up position.
        /// </summary>
        /// <returns>The current <see cref="RowPresenter"/>.</returns>
        RowPresenter ScrollToPageUp();

        /// <summary>
        /// Scrolls to row page down position.
        /// </summary>
        /// <returns>The current <see cref="RowPresenter"/>.</returns>
        RowPresenter ScrollToPageDown();
    }
}
