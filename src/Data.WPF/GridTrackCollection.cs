using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

        internal bool Classify(bool sizeToContent, ref T[] autoLengthTracks, ref T[] starLengthTracks)
        {
            var autoLengthCount = AutoLengthCount;
            if (sizeToContent)
                autoLengthCount += StarLengthCount;

            var starLengthCount = sizeToContent ? 0 : StarLengthCount;

            if (autoLengthTracks != null && autoLengthTracks.Length == autoLengthCount)
            {
                Debug.Assert(starLengthTracks != null && starLengthTracks.Length == starLengthCount);
                return false;
            }

            autoLengthTracks = autoLengthCount == 0 ? EmptyArray<T>.Singleton : new T[autoLengthCount];
            starLengthTracks = starLengthCount == 0 ? EmptyArray<T>.Singleton : new T[starLengthCount];
            DoClassify(sizeToContent, autoLengthTracks, starLengthTracks);
            return true;
        }

        private void DoClassify(bool sizeToContent, T[] autoLengthTracks, T[] starLengthTracks)
        {
            var indexAutoSize = 0;
            var indexStarSize = 0;
            foreach (var track in this)
            {
                var length = track.Length;
                if (length.IsAuto)
                    autoLengthTracks[indexAutoSize++] = track;
                else if (length.IsStar)
                {
                    if (sizeToContent)
                        autoLengthTracks[indexAutoSize++] = track;
                    else
                        starLengthTracks[indexStarSize++] = track;
                }
            }
        }
    }
}
