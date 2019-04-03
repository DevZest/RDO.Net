using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal struct Span
    {
        public readonly double Start;
        public readonly double End;

        public Span(double start, double end)
        {
            Debug.Assert(end >= start);
            Start = start;
            End = end;
        }

        public double Length
        {
            get { return End - Start; }
        }

        public Span[] Split(Span? gap)
        {
            if (!gap.HasValue)
                return new Span[] { this };

            var gapValue = gap.GetValueOrDefault();
            if (gapValue.End <= Start || gapValue.Start >= End)
                return new Span[] { this };

            var start1 = Start;
            var end1 = gapValue.Start;
            var start2 = gapValue.End;
            var end2 = End;

            if (end1 <= start1 && end2 <= start2)
                return null;
            else if (end1 <= start1)
                return new Span[] { new Span(start2, end2) };
            else if (end2 <= start2)
                return new Span[] { new Span(start1, end1) };
            else
                return new Span[] { new Span(start1, end1), new Span(start2, end2) };
        }
    }
}
