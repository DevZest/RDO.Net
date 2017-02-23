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

            public LogicalCrossTrack(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                GridTrack = gridTrack;
                _flowIndex = -1;
            }

            public LogicalCrossTrack(GridTrack gridTrack, int flowIndex)
            {
                Debug.Assert(gridTrack.IsRepeat && flowIndex >= 0);
                GridTrack = gridTrack;
                _flowIndex = flowIndex;
            }

            public readonly GridTrack GridTrack;
            private readonly int _flowIndex;
            public int FlowIndex
            {
                get { return IsRepeat ? _flowIndex : -1; }
            }

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
        }
    }
}
