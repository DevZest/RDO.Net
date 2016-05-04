using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal struct LogicalOffset
    {
        public readonly GridTrack GridTrack;
        public readonly int BlockStack;
        public readonly double Fraction;

        public LogicalOffset(GridTrack gridTrack, double fraction)
            : this(gridTrack, -1, fraction)
        {
        }

        public LogicalOffset(GridTrack gridTrack, int blockStack, double fraction)
        {
            Debug.Assert(gridTrack != null);
            Debug.Assert(fraction >= 0 && fraction <= 1);
            GridTrack = gridTrack;
            BlockStack = blockStack;
            Fraction = fraction;
        }

        private LayoutManagerXY LayoutManager
        {
            get { return GridTrack.Template.LayoutManager as LayoutManagerXY; }
        }
    }
}
