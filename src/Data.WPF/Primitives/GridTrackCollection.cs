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
    }
}
