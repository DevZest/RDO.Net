using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class GridLine
    {
        internal GridLine(GridPoint startGridPoint, GridPoint endGridPoint, Pen pen, GridLinePosition position)
        {
            StartGridPoint = startGridPoint;
            EndGridPoint = endGridPoint;
            Pen = pen;
            Position = position;
        }

        public GridPoint StartGridPoint { get; private set; }

        public GridPoint EndGridPoint { get; private set; }

        public Pen Pen { get; private set; }

        public GridLinePosition Position { get; private set; }
    }
}
