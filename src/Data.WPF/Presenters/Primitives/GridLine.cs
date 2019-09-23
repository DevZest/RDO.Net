using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents the grid line definition that will be rendered on the view.
    /// </summary>
    public sealed class GridLine
    {
        internal GridLine(GridPoint startGridPoint, GridPoint endGridPoint, Pen pen, GridPlacement? placement)
        {
            StartGridPoint = startGridPoint;
            EndGridPoint = endGridPoint;
            Pen = pen;
            Placement = placement;
        }

        /// <summary>
        /// Gets the start grid point of the line.
        /// </summary>
        public GridPoint StartGridPoint { get; private set; }

        /// <summary>
        /// Gets lthe end grid point of the line.
        /// </summary>
        public GridPoint EndGridPoint { get; private set; }

        /// <summary>
        /// Gets the <see cref="Pen"/> that will be used to render the line.
        /// </summary>
        public Pen Pen { get; private set; }

        /// <summary>
        /// Gets the grid placement of the line.
        /// </summary>
        public GridPlacement? Placement { get; private set; }

        /// <summary>
        /// Gets the orientation of the line.
        /// </summary>
        public Orientation Orientation
        {
            get { return StartGridPoint.Y == EndGridPoint.Y ? Orientation.Horizontal : Orientation.Vertical; }
        }
    }
}
