using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class GridTrackCollection<T> : ReadOnlyCollection<T>, IGridTrackCollection
        where T : GridTrack, IConcatList<T>
    {
        internal GridTrackCollection(Template template)
            : base(new List<T>())
        {
            Template = template;
        }

        public Template Template { get; private set; }

        public abstract Orientation Orientation { get; }

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

        public double TotalAutoLength { get; set; }

        internal double TotalStarFactor { get; private set; }

        GridTrack IReadOnlyList<GridTrack>.this[int index]
        {
            get { return this[index]; }
        }

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

        internal void InitMeasuredAutoLengths(bool sizeToContent)
        {
            TotalAutoLength = 0;
            Filter(x => x.IsAutoLength(sizeToContent)).ForEach(x => x.MeasuredLength = 0);
        }

        internal double GetMeasuredLength(int startIndex, int endIndex)
        {
            Debug.Assert(startIndex >= 0 && startIndex <= endIndex && endIndex < Count);
            return this[endIndex].EndOffset - this[startIndex].StartOffset;
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

        private bool _isOffsetValid = true;

        void IGridTrackCollection.InvalidateOffset()
        {
            _isOffsetValid = false;
        }

        void IGridTrackCollection.RefreshOffset()
        {
            if (_isOffsetValid)
                return;

            _isOffsetValid = true;  // prevent re-entrance
            for (int i = 1; i < Count; i++)
                this[i].StartOffset = this[i - 1].EndOffset;
        }

        IEnumerator<GridTrack> IEnumerable<GridTrack>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }
    }
}
