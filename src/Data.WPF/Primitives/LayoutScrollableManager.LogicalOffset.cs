using System;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutScrollableManager
    {
        private struct LogicalOffset
        {
            public LogicalOffset(double value)
            {
                _value = value;
            }

            public LogicalOffset(int gridOffset, double fractionOffset)
            {
                Debug.Assert(gridOffset >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset < 1);
                _value = gridOffset + fractionOffset;
            }

            private readonly double _value;
            public double Value
            {
                get { return _value; }
            }

            public int GridOffset
            {
                get { return (int)_value; }
            }

            public double FractionOffset
            {
                get { return _value - GridOffset; }
            }
        }

        private double GetOffset(LogicalOffset logicalOffset)
        {
            var gridOffset = logicalOffset.GridOffset;
            if (gridOffset >= MaxGridOffsetMain)
                return GetLogicalGridTrack(MaxGridOffsetMain - 1).Span.End;
            else
            {
                var span = GetLogicalGridTrack(gridOffset).Span;
                return span.Start + span.Length * logicalOffset.FractionOffset;
            }
        }

        private LogicalOffset GetLogicalOffset(double offset)
        {
            const double Epsilon = 1e-8;

            if (offset < 0)
                offset = 0;

            // Binary search
            var min = 0;
            var max = MaxGridOffsetMain - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                var span = GetLogicalGridTrack(mid).Span;
                if (offset < span.Start - Epsilon)
                    max = mid - 1;
                else if (offset >= span.End)
                    min = mid + 1;
                else
                    return new LogicalOffset(mid, Math.Max(0, offset - span.Start) / span.Length);
            }

            return new LogicalOffset(MaxGridOffsetMain);
        }
    }
}
