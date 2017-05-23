using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Windows.Primitives
{
    public sealed class GridLine
    {
        internal GridLine(GridPoint startGridPoint, GridPoint endGridPoint, Pen pen, GridPlacement? placement)
        {
            StartGridPoint = startGridPoint;
            EndGridPoint = endGridPoint;
            Pen = pen;
            Placement = placement;
        }

        public GridPoint StartGridPoint { get; private set; }

        public GridPoint EndGridPoint { get; private set; }

        public Pen Pen { get; private set; }

        public GridPlacement? Placement { get; private set; }

        public Orientation Orientation
        {
            get { return StartGridPoint.Y == EndGridPoint.Y ? Orientation.Horizontal : Orientation.Vertical; }
        }
    }
}
