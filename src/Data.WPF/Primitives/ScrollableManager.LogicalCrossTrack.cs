using System;
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
                Debug.Assert(gridTrack.IsRepeat || flowIndex == 0);
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
                get { return GridTrack != null && GridTrack.IsRepeat; }
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

            //private double StartExtent
            //{
            //    get
            //    {
            //        Debug.Assert(!IsEof);

            //        var result = GridTrack.StartOffset;

            //        if (GridTrack.IsTail && FlowCount > 1)
            //            result += (FlowCount - 1) * FlowLength;
            //        else if (GridTrack.IsRepeat && FlowIndex > 0)
            //            result += FlowIndex * FlowLength;

            //        return result;
            //    }
            //}

            public double Length
            {
                get { return GridTrack.MeasuredLength; }
            }

            public double StartLocation
            {
                get
                {
                    Debug.Assert(!IsEof);

                    var result = GridTrack.StartOffset;

                    if (FlowIndex == 0 && GridTrack.IsFrozenHead)
                        return result;

                    result -= ScrollableManager.ScrollOffsetCross;
                    if (FlowIndex > 0)
                        result += FlowIndex * FlowLength;


                    if (FlowIndex == FlowCount - 1 && GridTrack.IsFrozenTail)
                    {
                        double max = ScrollableManager.ViewportCross - (GridTracksCross.LastOf().EndOffset - GridTrack.StartOffset);
                        if (result > max)
                            result = max;
                    }

                    return result;
                }
            }

            public double EndLocation
            {
                get { return StartLocation + Length; }
            }
        }

        private double GetStartLocationCross(GridTrack gridTrack, int flowIndex)
        {
            if (gridTrack.IsRepeat || flowIndex == 0)
                return new LogicalCrossTrack(gridTrack, flowIndex).StartLocation;
            else
                return new LogicalCrossTrack(gridTrack, 0).StartLocation + flowIndex * FlowLength;

        }

        private double GetEndLocationCross(GridTrack gridTrack, int flowIndex)
        {
            return GetStartLocationCross(gridTrack, flowIndex) + gridTrack.MeasuredLength;
        }

        private double GetStartLocationCross(GridRange gridRange, int flowIndex)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).StartTrack;
            return new LogicalCrossTrack(gridTrack, flowIndex).StartLocation;
        }

        private double GetEndLocationCross(GridRange gridRange, int flowIndex)
        {
            var gridTrack = GridTracksCross.GetGridSpan(gridRange).EndTrack;
            return new LogicalCrossTrack(gridTrack, flowIndex).EndLocation;
        }

        private double GetStartLocationCross(int flowIndex)
        {
            var rowRange = Template.RowRange;
            var result = GetStartLocationCross(rowRange, flowIndex);
            if (flowIndex == FlowCount - 1 && GridTracksCross.GetGridSpan(rowRange).EndTrack.IsFrozenTail)
                result = Math.Min(ViewportCross - FrozenTailLengthCross, result);
            return result;
        }

        private double GetEndLocationCross(int flowIndex)
        {
            return GetEndLocationCross(Template.RowRange, flowIndex);
        }
    }
}
