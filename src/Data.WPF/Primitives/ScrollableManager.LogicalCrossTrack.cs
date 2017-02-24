using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class ScrollableManager
    {
        private struct LogicalCrossTrack
        {
            public static LogicalCrossTrack Eof
            {
                get { return new LogicalCrossTrack(); }
            }

            public LogicalCrossTrack(GridTrack gridTrack, int flowIndex = 0)
            {
                Debug.Assert(gridTrack != null && flowIndex >= 0);
                Debug.Assert(gridTrack.IsContainer || flowIndex == 0);
                GridTrack = gridTrack;
                FlowIndex = flowIndex;
            }

            public readonly GridTrack GridTrack;
            private readonly int FlowIndex;

            public bool IsEof
            {
                get { return GridTrack == null; }
            }

            public bool IsRepeat
            {
                get { return GridTrack != null && GridTrack.IsContainer; }
            }

            public static bool operator ==(LogicalCrossTrack x, LogicalCrossTrack y)
            {
                return x.GridTrack == y.GridTrack && x.FlowIndex == y.FlowIndex;
            }

            public static bool operator !=(LogicalCrossTrack x, LogicalCrossTrack y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return IsEof ? 0 : GridTrack.GetHashCode() ^ FlowIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is LogicalMainTrack ? (LogicalCrossTrack)obj == this : false;
            }

            private Template Template
            {
                get { return GridTrack.Template; }
            }

            private ScrollableManager ScrollableManager
            {
                get { return Template.ScrollableManager; }
            }

            private IGridTrackCollection GridTracksCross
            {
                get { return ScrollableManager.GridTracksCross; }
            }

            private int FlowCount
            {
                get { return ScrollableManager.FlowCount; }
            }

            private double FlowLength
            {
                get { return ScrollableManager.FlowLength; }
            }

            private double StartExtent
            {
                get
                {
                    Debug.Assert(!IsEof);

                    var result = GridTrack.StartOffset;

                    if (GridTrack.IsTail && FlowCount > 1)
                        result += (FlowCount - 1) * FlowLength;
                    else if (GridTrack.IsContainer && FlowIndex > 0)
                        result += FlowIndex * FlowLength;

                    return result;
                }
            }

            public double Length
            {
                get { return GridTrack.MeasuredLength; }
            }

            public double StartPosition
            {
                get
                {
                    Debug.Assert(!IsEof);

                    var result = StartExtent;

                    if (FlowIndex == 0 && GridTrack.IsFrozenHead)
                        return result;

                    result -= ScrollableManager.ScrollOffsetCross;

                    if (FlowIndex == FlowCount - 1 && GridTrack.IsFrozenTail)
                    {
                        double max = ScrollableManager.ViewportCross - (GridTracksCross.LastOf().EndOffset - GridTrack.StartOffset);
                        if (result > max)
                            result = max;
                    }

                    return result;
                }
            }

            public double EndPosition
            {
                get { return StartPosition + Length; }
            }
        }

        private double GetStartPositionCross(GridTrack gridTrack, int flowIndex)
        {
            return new LogicalCrossTrack(gridTrack, flowIndex).StartPosition;
        }

        private double GetEndPositionCross(GridTrack gridTrack, int flowIndex)
        {
            return GetStartPositionCross(gridTrack, flowIndex) + gridTrack.MeasuredLength;
        }

        private double GetStartPositionCross(GridRange gridRange, int flowIndex)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).StartTrack;
            return new LogicalCrossTrack(gridTrack, flowIndex).StartPosition;
        }

        private double GetEndPositionCross(GridRange gridRange, int flowIndex)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).EndTrack;
            return new LogicalCrossTrack(gridTrack, flowIndex).EndPosition;
        }
    }
}
