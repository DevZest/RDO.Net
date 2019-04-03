using System;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
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

            public bool IsHead
            {
                get { return GridTrack != null && GridTrack.IsHead; }
            }

            public bool IsContainer
            {
                get { return GridTrack != null && GridTrack.IsContainer; }
            }

            public bool IsTail
            {
                get { return GridTrack != null && GridTrack.IsTail; }
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

            private int FlowRepeatCount
            {
                get { return ScrollableManager.FlowRepeatCount; }
            }

            private double FlowLength
            {
                get { return ScrollableManager.FlowLength; }
            }

            public double StartExtent
            {
                get
                {
                    Debug.Assert(!IsEof);

                    var result = GridTrack.StartOffset;

                    if (GridTrack.IsTail && FlowRepeatCount > 1)
                        result += (FlowRepeatCount - 1) * FlowLength;
                    else if (GridTrack.IsContainer && FlowIndex > 0)
                        result += FlowIndex * FlowLength;

                    return result;
                }
            }

            public double EndExtent
            {
                get { return StartExtent + Length; }
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

                    if (FlowIndex == FlowRepeatCount - 1 && GridTrack.IsFrozenTail)
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

            private int ContainerTracksCount
            {
                get { return GridTracksCross.ContainerTracksCount; }
            }

            public int StartGridExtent
            {
                get
                {
                    Debug.Assert(!IsEof);

                    if (GridTrack.IsHead)
                        return GridTrack.Ordinal;
                    else if (IsContainer)
                        return GridTrack.Ordinal + FlowIndex * ContainerTracksCount;
                    else
                    {
                        Debug.Assert(IsTail);
                        return GridTrack.Ordinal + (FlowRepeatCount - 1) * ContainerTracksCount;
                    }
                }
            }

            public int EndGridExtent
            {
                get { return StartGridExtent + 1; }
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

        private LogicalCrossTrack GetLogicalCrossTrack(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);

            if (gridExtent >= MaxGridExtentCross)
                return LogicalCrossTrack.Eof;

            var index = 0;
            var beforeRowGridExtent = GridTracksCross.RowStart.Ordinal;
            if (gridExtent < beforeRowGridExtent)
                return new LogicalCrossTrack(GridTracksCross[gridExtent]);

            index += beforeRowGridExtent;
            gridExtent -= beforeRowGridExtent;
            var rowTracksCount = GridTracksCross.RowTracksCount;
            var rowGridExtent = rowTracksCount * FlowRepeatCount;
            if (gridExtent < rowGridExtent)
                return new LogicalCrossTrack(GridTracksCross[index + gridExtent % rowTracksCount], gridExtent / rowTracksCount);

            index += GridTracksCross.RowTracksCount;
            gridExtent -= rowGridExtent;
            return new LogicalCrossTrack(GridTracksCross[index + gridExtent]);
        }
    }
}
