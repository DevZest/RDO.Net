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
                AbsoluteLengthTotal += item.MeasuredLength;
            }
        }

        internal double AbsoluteLengthTotal { get; private set; }

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
                    result = result.Concat(track);
                if (action != null)
                    action(track);
            }
            return result;
        }
    }
}
