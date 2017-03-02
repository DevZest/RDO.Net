using System;

namespace DevZest.Windows.Data.Primitives
{
    internal struct Clip
    {
        public readonly double Head;
        public readonly double Tail;

        public Clip(double start, double end, double? minStart, double? maxEnd)
        {
            Head = GetClipHead(start, end, minStart);
            Tail = GetClipTail(start, end, maxEnd);
        }

        internal static bool IsClipped(double value, double? minStart, double? maxEnd)
        {
            return IsHeadClipped(value, minStart) || IsTailClipped(value, maxEnd);
        }

        internal static bool IsHeadClipped(double value, double? minStart)
        {
            return GetClipHead(value, value, minStart) > 0;
        }

        internal static bool IsTailClipped(double value, double? maxEnd)
        {
            return GetClipTail(value, value, maxEnd) > 0;
        }

        private static double GetClipHead(double start, double end, double? minStart)
        {
            if (!minStart.HasValue)
                return 0;

            var minStartValue = minStart.GetValueOrDefault();
            if (end <= minStartValue)
                return double.PositiveInfinity;

            return minStartValue > start ? minStartValue - start : 0;
        }

        private static double GetClipTail(double start, double end, double? maxEnd)
        {
            if (!maxEnd.HasValue)
                return 0;

            var maxEndValue = maxEnd.GetValueOrDefault();
            if (start >= maxEndValue)
                return double.PositiveInfinity;

            return end > maxEndValue ? end - maxEndValue : 0;
        }

        private Clip(double head, double tail)
        {
            Head = head;
            Tail = tail;
        }

        public Clip Merge(Clip clip)
        {
            return new Clip(Math.Max(this.Head, clip.Head), Math.Max(this.Tail, clip.Tail));
        }
    }
}
