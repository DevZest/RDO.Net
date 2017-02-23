using System;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    partial class ScrollableManager
    {
        private struct LogicalExtent
        {
            public LogicalExtent(double value)
            {
                _value = value;
            }

            public LogicalExtent(int gridExtent, double fractionOffset)
            {
                Debug.Assert(gridExtent >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset <= 1);
                _value = gridExtent + fractionOffset;
            }

            private readonly double _value;
            public double Value
            {
                get { return _value; }
            }

            public int GridExtent
            {
                get { return (int)_value; }
            }

            public double Fraction
            {
                get { return _value - GridExtent; }
            }
        }

        private double Translate(LogicalExtent logicalExtent)
        {
            var gridExtent = logicalExtent.GridExtent;
            if (gridExtent >= MaxGridExtentMain)
                return GetLogicalMainTrack(MaxGridExtentMain - 1).ExtentSpan.End;
            else
            {
                var span = GetLogicalMainTrack(gridExtent).ExtentSpan;
                return span.Start + span.Length * logicalExtent.Fraction;
            }
        }

        private LogicalExtent Translate(double extent)
        {
            const double Epsilon = 1e-8;

            if (extent < 0)
                extent = 0;

            // Binary search
            var min = 0;
            var max = MaxGridExtentMain - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                var span = GetLogicalMainTrack(mid).ExtentSpan;
                if (extent < span.Start - Epsilon)
                    max = mid - 1;
                else if (extent >= span.End)
                    min = mid + 1;
                else
                    return new LogicalExtent(mid, Math.Max(0, extent - span.Start) / span.Length);
            }

            return new LogicalExtent(MaxGridExtentMain);
        }
    }
}
