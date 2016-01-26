using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest.Data.Windows
{
    public class GridTrackCollection<T> : ReadOnlyCollection<T>
        where T : GridTrack
    {
        internal int StarLengthCount { get; private set; }

        internal int AutoLengthCount { get; private set; }

        internal int AbsoluteLengthCount
        {
            get { return Count - StarLengthCount - AutoLengthCount; }
        }

        internal double AbsoluteLengthTotal { get; private set; }

        internal GridTrackCollection()
            : base(new List<T>())
        {
        }

        internal void Add(T item)
        {
            Items.Add(item);

            if (item.Length.IsAuto)
                AutoLengthCount++;
            else if (item.Length.IsStar)
                StarLengthCount++;
            else
            {
                item.MeasuredLength = item.Length.Value;
                AbsoluteLengthTotal += item.MeasuredLength;
            }
        }
    }
}
