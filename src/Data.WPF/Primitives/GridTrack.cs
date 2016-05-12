using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class GridTrack
    {
        internal GridTrack(IGridTrackCollection owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
            VariantAutoLengthIndex = -1;
        }

        internal IGridTrackCollection Owner { get; private set; }

        internal Template Template
        {
            get { return Owner.Template; }
        }

        internal LayoutXYManager LayoutXYManager
        {
            get { return Template.LayoutXYManager; }
        }

        internal int Ordinal { get; private set; }

        public Orientation Orientation
        {
            get { return Owner.Orientation; }
        }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        private bool sizeToContent
        {
            get { return Owner.SizeToContent; }
        }

        internal bool IsAutoLength
        {
            get { return Length.IsAuto || (Length.IsStar && sizeToContent); }
        }

        internal bool IsStarLength
        {
            get { return Length.IsStar && !sizeToContent; }
        }

        private double _measuredLength;
        internal double MeasuredLength
        {
            get { return IsVariantAutoLength ? 0 : _measuredLength; }
        }

        internal void SetMeasuredLength(double value)
        {
            Debug.Assert(!IsVariantAutoLength);
            if (_measuredLength == value)
                return;

            _measuredLength = value;
            Owner.InvalidateOffset();
        }

        internal double AvgVariantAutoLength
        {
            get
            {
                Debug.Assert(IsVariantAutoLength);
                LayoutXYManager.RefreshVariantAutoLengths();
                return _measuredLength;
            }
        }

        internal void SetAvgVariantAutoLength(double value)
        {
            Debug.Assert(IsVariantAutoLength);
            _measuredLength = value;
        }

        private double _startOffset;
        internal double StartOffset
        {
            get
            {
                Owner.RefreshOffset();
                return _startOffset;
            }
            set { _startOffset = value; }
        }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal int VariantAutoLengthIndex { get; set; }

        internal bool IsVariantAutoLength
        {
            get { return VariantAutoLengthIndex >= 0; }
        }

        internal void VerifyUnitType()
        {
            if (Length.IsAbsolute || !Template.Orientation.HasValue)
                return;

            if (Length.IsStar)
                VerifyStarLength();
            else
            {
                Debug.Assert(Length.IsAuto);
                VerifyAutoLength();
            }
        }

        private void VerifyStarLength()
        {
            Debug.Assert(Length.IsStar && Template.Orientation.HasValue);

            var layoutOrientation = Template.Orientation.Value;
            if (Orientation == layoutOrientation || Template.BlockDimensions != 1)
                throw new InvalidOperationException(InvalidStarLengthMessage);
        }

        internal abstract string InvalidStarLengthMessage { get; }

        private void VerifyAutoLength()
        {
            Debug.Assert(Length.IsAuto);

            if (Template.IsMultidimensional(Orientation))
                throw new InvalidOperationException(InvalidAutoLengthMessage);
        }

        internal abstract string InvalidAutoLengthMessage { get; }

        internal bool IsHead
        {
            get { return Ordinal < Owner.BlockStart.Ordinal; }
        }

        internal bool IsRepeat
        {
            get { return Ordinal >= Owner.BlockStart.Ordinal && Ordinal <= Owner.BlockEnd.Ordinal; }
        }

        internal bool IsTail
        {
            get { return Ordinal > Owner.BlockEnd.Ordinal; }
        }

        private IReadOnlyList<GridTrack> VariantAutoLengthTracks
        {
            get { return LayoutXYManager.VariantAutoLengthTracks; }
        }

        private BlockViewCollection BlockViews
        {
            get { return LayoutXYManager.BlockViews; }
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int MaxFrozenHead
        {
            get { return Owner.MaxFrozenHead; }
        }

        private double TotalDeltaVariantLength
        {
            get
            {
                Debug.Assert(BlockViews.Count > 0);
                return BlockViews.Last.EndMeasuredAutoLengthOffset - BlockViews.First.StartMeasuredAutoLengthOffset;
            }
        }

        private double AvgDeltaVariantLength
        {
            get { return BlockViews.Count == 0 ? 0 : TotalDeltaVariantLength / BlockViews.Count; }
        }

        private double FixBlockLength
        {
            get { return Owner.BlockEnd.EndOffset - Owner.BlockStart.StartOffset; }
        }

        private double AvgBlockLength
        {
            get { return FixBlockLength + AvgDeltaVariantLength; }
        }

        internal Span GetSpan()
        {
            Debug.Assert(Owner == LayoutXYManager.GridTracksMain);
            Debug.Assert(!IsRepeat);

            if (IsHead)
                return new Span(StartOffset, EndOffset);

            Debug.Assert(IsTail);
            var delta = GetBlocksLength(MaxBlockCount) - Owner.GetMeasuredLength(Template.BlockRange);
            return new Span(StartOffset + delta, EndOffset + delta);
        }

        internal Span GetSpan(int blockOrdinal)
        {
            Debug.Assert(Owner == LayoutXYManager.GridTracksMain);
            Debug.Assert(IsRepeat && blockOrdinal >= 0);

            var relativeSpan = GetRelativeSpan(blockOrdinal);
            var startOffset = Owner[MaxFrozenHead].StartOffset + GetBlocksLength(blockOrdinal);
            return new Span(startOffset + relativeSpan.StartOffset, startOffset + relativeSpan.EndOffset);
        }

        private double GetBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= MaxBlockCount);
            if (count == 0)
                return 0;

            var realized = BlockViews.Count;
            var unrealized = Math.Max(0, count - realized);
            return GetRealizedBlocksLength(realized) + unrealized * AvgBlockLength;
        }

        private double GetRealizedBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= BlockViews.Count);
            return count == 0 ? 0 : count * FixBlockLength + BlockViews[count - 1].EndMeasuredAutoLengthOffset;
        }

        internal Span GetRelativeSpan(BlockView block)
        {
            Debug.Assert(Owner == LayoutXYManager.GridTracksMain);
            Debug.Assert(block != null && IsRepeat);
            return GetRelativeSpan(block.Ordinal);
        }

        private Span GetRelativeSpan(int blockOrdinal)
        {
            Debug.Assert(IsRepeat && blockOrdinal >= 0);
            var startTrack = Owner.BlockStart;

            var startOffset = StartOffset - startTrack.StartOffset;
            var endOffset = EndOffset - startTrack.StartOffset;

            var variantAutoLengthTrack = LastVariantAutoLengthTrack;
            if (variantAutoLengthTrack != null)
            {
                var variantLengthSpan = variantAutoLengthTrack.GetVariantLengthSpan(blockOrdinal);
                startOffset += variantLengthSpan.StartOffset;
                endOffset += variantLengthSpan.EndOffset;
            }
            return new Span(startOffset, endOffset);
        }

        private Span GetVariantLengthSpan(int blockOrdinal)
        {
            Debug.Assert(IsVariantAutoLength);

            double startOffset, endOffset;

            var blockView = BlockViews.GetBlockView(blockOrdinal);
            if (blockView != null)
            {
                startOffset = blockView.GetMeasuredAutoLengthStartOffset(this);
                endOffset = blockView.GetMeasuredAutoLengthEndOffset(this);
            }
            else
            {
                endOffset = 0;
                for (int i = 0; i <= VariantAutoLengthIndex; i++)
                    endOffset += VariantAutoLengthTracks[i].AvgVariantAutoLength;
                startOffset = endOffset - AvgVariantAutoLength;
            }

            return new Span(startOffset, endOffset);
        }

        private GridTrack LastVariantAutoLengthTrack
        {
            get
            {
                Debug.Assert(IsRepeat);

                // optimization: shortcuts for common cases
                if (VariantAutoLengthTracks.Count == 0)
                    return null;
                if (this == Owner.BlockStart)
                    return IsVariantAutoLength ? this : null;
                if (this == Owner.BlockEnd)
                    return VariantAutoLengthTracks.LastOf(1);

                GridTrack result = null;
                for (int i = 0; i < VariantAutoLengthTracks.Count; i++)
                {
                    var variantAutoLengthTrack = VariantAutoLengthTracks[i];
                    if (variantAutoLengthTrack.Ordinal > Ordinal)
                        break;

                    result = variantAutoLengthTrack;
                }
                return result;
            }
        }
    }
}
