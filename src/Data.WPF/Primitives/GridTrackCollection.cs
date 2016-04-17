using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    public class GridTrackCollection<T> : ReadOnlyCollection<T>
        where T : GridTrack
    {
        internal GridTrackCollection()
            : base(new List<T>())
        {
        }

        internal void Add(T item)
        {
            Items.Add(item);

            if (item.Length.IsAbsolute)
            {
                item.MeasuredLength = item.Length.Value;
                TotalAbsoluteLength += item.MeasuredLength;
            }
            else if (item.Length.IsStar)
                TotalStarFactor += item.Length.Value;
        }

        internal double TotalAbsoluteLength { get; private set; }

        internal double TotalAutoLength { get; set; }

        internal double TotalStarFactor { get; private set; }

        internal IConcatList<T> Filter(Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(0, Count - 1, predict, action);
        }

        internal IConcatList<T> Filter(int startIndex, int endIndex, Func<T, bool> predict, Action<T> action = null)
        {
            var result = ConcatList<T>.Empty;
            for (int i = startIndex; i <= endIndex; i++)
            {
                var track = this[i];
                if (predict(track))
                    result = result.Concat((IConcatList<T>)track);
                if (action != null)
                    action(track);
            }
            return result;
        }

        internal void InitMeasuredAutoLengths(bool sizeToContent)
        {
            TotalAutoLength = 0;
            Filter(x => x.IsAutoLength(sizeToContent)).ForEach(x => x.MeasuredLength = 0);
        }

        internal double GetMeasuredLength(int startIndex, int endIndex)
        {
            Debug.Assert(startIndex >= 0 && startIndex <= endIndex && endIndex < Count);
            return this[endIndex].MeasuredEndOffset - this[startIndex].MeasuredStartOffset;
        }

        internal double GetMeasuredLength(int startIndex, int endIndex, Func<T, bool> predict)
        {
            Debug.Assert(startIndex >= 0 && startIndex <= endIndex && endIndex < Count);
            double result = 0d;
            for (int i = startIndex; i <= endIndex; i++)
            {
                var track = this[i];
                if (predict == null || predict(track))
                    result += track.MeasuredLength;
            }
            return result;
        }

        internal void RefreshMeasuredOffset()
        {
            for (int i = 1; i < Count; i++)
                this[i].MeasuredStartOffset = this[i - 1].MeasuredStartOffset + this[i - 1].MeasuredLength;
        }
    }
}
