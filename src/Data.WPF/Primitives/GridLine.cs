using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class GridLine
    {
        internal GridLine(Orientation orientation, int startGridOffset, int endGridOffset, Pen pen, GridLinePosition position)
        {
            Orientation = orientation;
            StartGridOffset = startGridOffset;
            EndGridOffset = endGridOffset;
            Pen = pen;
            Position = position;
        }

        public Orientation Orientation { get; private set; }

        public int StartGridOffset { get; private set; }

        public int EndGridOffset { get; private set; }

        public Pen Pen { get; private set; }

        public GridLinePosition Position { get; private set; }
    }
}
