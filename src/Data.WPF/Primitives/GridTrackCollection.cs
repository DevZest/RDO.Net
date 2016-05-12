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

        GridSpan IGridTrackCollection.GetGridSpan(GridRange gridRange)
        {
            var result = GetGridSpan(gridRange);
            return new GridSpan(result.StartTrack, result.EndTrack);
        }

        public void Add(T item)
        {
            Items.Add(item);

            if (item.Length.IsAbsolute)
                TotalAbsoluteLength += item.Length.Value;
            else if (item.Length.IsStar)
                TotalStarFactor += item.Length.Value;
        }

        public double TotalAbsoluteLength { get; private set; }

        public double TotalAutoLength { get; set; }

        public double TotalStarFactor { get; private set; }

        private IConcatList<T> Filter(Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(GridSpan<T>.From(this), predict, action);
        }

        public IConcatList<T> Filter(GridRange gridRange, Func<T, bool> predict, Action<T> action = null)
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

        internal void InitMeasuredLengths()
        {
            TotalAutoLength = 0;
            foreach (var gridTrack in this)
            {
                if (gridTrack.IsAutoLength || gridTrack.IsVariantLength || gridTrack.IsStarLength)
                    gridTrack.MeasuredLength = 0;
                else
                    gridTrack.MeasuredLength = gridTrack.Length.Value;
            }
        }

        public double GetMeasuredLength(GridRange gridRange)
        {
            return GetMeasuredLength(GetGridSpan(gridRange));
        }

        internal double GetMeasuredLength(GridSpan<T> gridSpan)
        {
            if (gridSpan.IsEmpty)
                return 0;

            var startOrdinal = gridSpan.StartTrack.Ordinal;
            var endOrdinal = gridSpan.EndTrack.Ordinal;
            Debug.Assert(startOrdinal >= 0 && startOrdinal <= endOrdinal && endOrdinal < Count);
            return this[endOrdinal].EndOffset - this[startOrdinal].StartOffset;
        }

        internal double GetMeasuredLength(GridSpan<T> gridSpan, Func<T, bool> predict)
        {
            if (gridSpan.IsEmpty)
                return 0;

            var startOrdinal = gridSpan.StartTrack.Ordinal;
            var endOrdinal = gridSpan.EndTrack.Ordinal;
            Debug.Assert(startOrdinal >= 0 && startOrdinal <= endOrdinal && endOrdinal < Count);
            double result = 0d;
            for (int i = startOrdinal; i <= endOrdinal; i++)
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

        GridTrack IReadOnlyList<GridTrack>.this[int index]
        {
            get { return this[index]; }
        }

        IEnumerator<GridTrack> IEnumerable<GridTrack>.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IConcatList<T> _starLengthTracks;
        private IConcatList<T> StarLengthTracks
        {
            get
            {
                if (_starLengthTracks == null)
                    _starLengthTracks = Filter(x => x.IsStarLength);
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

        private GridSpan<T> BlockSpan
        {
            get { return GetGridSpan(Template.RowRange); }
        }

        private IConcatList<T> InitVariantLengthTracks()
        {
            var blockSpan = BlockSpan;
            var result = Filter(blockSpan, x => x.Length.IsAuto || (x.Length.IsAbsolute && x.IsVariant));
            for (int i = 0; i < result.Count; i++)
                result[i].VariantLengthIndex = i;
            return result;
        }

        IReadOnlyList<GridTrack> IGridTrackCollection.InitVariantLengthTracks()
        {
            return InitVariantLengthTracks();
        }

        protected abstract double BlockDimensionLength { get; }

        public Vector BlockDimensionVector
        {
            get { return ToVector(0, BlockDimensionLength); }
        }

        public abstract int FrozenHead { get; }

        protected abstract string FrozenHeadName { get; }

        public abstract int FrozenTail { get; }

        protected abstract string FrozenTailName { get; }

        public int MaxFrozenHead
        {
            get { return BlockStart.Ordinal; }
        }

        public int MaxFrozenTail
        {
            get { return Count - 1 - BlockEnd.Ordinal; }
        }

        private int Stretches
        {
            get { return Template.Stretches; }
        }

        internal void VerifyFrozenMargins()
        {
            if (FrozenHead > MaxFrozenHead)
                throw new InvalidOperationException(Strings.Template_InvalidFrozenMargin(FrozenHeadName));
            if (FrozenTail > MaxFrozenTail)
                throw new InvalidOperationException(Strings.Template_InvalidFrozenMargin(FrozenTailName));
            if (Stretches > FrozenTail)
                throw new InvalidOperationException(Strings.Template_InvalidStretches(FrozenTailName));
        }

        public GridTrack BlockStart
        {
            get { return BlockSpan.StartTrack; }
        }

        public GridTrack BlockEnd
        {
            get { return BlockSpan.EndTrack; }
        }

        public abstract bool SizeToContent { get; }

        public abstract double AvailableLength { get; }

        public abstract Vector ToVector(double valueMain, double valueCross);
    }
}
