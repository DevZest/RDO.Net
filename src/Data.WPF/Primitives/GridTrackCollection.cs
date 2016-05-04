using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
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

        protected abstract GridSpan<T> GetGridSpan(GridRange gridRange);

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

        private IConcatList<T> Filter(Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(GridSpan<T>.From(this), predict, action);
        }

        internal IConcatList<T> Filter(GridRange gridRange, Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(GetGridSpan(gridRange), predict, action);
        }

        private IConcatList<T> Filter(GridSpan<T> gridSpan, Func<T, bool> predict, Action<T> action = null)
        {
            var result = ConcatList<T>.Empty;
            if (gridSpan.IsEmpty)
                return result;

            var startIndex = gridSpan.StartTrack.Ordinal;
            var endIndex = gridSpan.EndTrack.Ordinal;
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

        protected abstract bool SizeToContent { get; }

        protected abstract double AvailableLength { get; }

        private IConcatList<T> _starLengthTracks;
        private IConcatList<T> StarLengthTracks
        {
            get
            {
                if (_starLengthTracks == null)
                    _starLengthTracks = Filter(x => x.IsStarLength(SizeToContent));
                return _starLengthTracks;
            }
        }

        public void InvalidateStarLengthTracks()
        {
            _starLengthTracks = null;
        }

        public void DistributeStarLengths()
        {
            if (Count == 0)
                return;

            var totalLength = Math.Max(0d, AvailableLength - TotalAbsoluteLength - TotalAutoLength);
            var totalStarFactor = TotalStarFactor;
            foreach (var gridTrack in StarLengthTracks)
                gridTrack.MeasuredLength = totalLength * (gridTrack.Length.Value / totalStarFactor);
        }

        private IConcatList<T> InitVariantAutoLengthTracks()
        {
            var blockSpan = GetGridSpan(Template.RowRange);
            var result = Filter(blockSpan, x => x.Length.IsAuto);
            for (int i = 0; i < result.Count; i++)
                result[i].VariantAutoLengthIndex = i;
            return result;
        }

        IReadOnlyList<GridTrack> IGridTrackCollection.InitVariantAutoLengthTracks()
        {
            return InitVariantAutoLengthTracks();
        }

        public abstract Vector BlockDimensionVector { get; }
    }
}
