using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public sealed class GridLine
    {
        internal GridLine(Orientation orientation, int startGridOffset, int endGridOffset, Brush brush, double thickness, GridLinePosition position)
        {
            Orientation = orientation;
            StartGridOffset = startGridOffset;
            EndGridOffset = endGridOffset;
            Brush = brush;
            Thickness = thickness;
            Position = position;
        }

        public Orientation Orientation { get; private set; }

        public int StartGridOffset { get; private set; }

        public int EndGridOffset { get; private set; }

        public Brush Brush { get; private set; }

        public double Thickness { get; private set; }

        public GridLinePosition Position { get; private set; }
    }
}
