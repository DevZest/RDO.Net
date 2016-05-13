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
        }

        internal IGridTrackCollection Owner { get; private set; }

        internal Template Template
        {
            get { return Owner.Template; }
        }

        internal int Ordinal { get; private set; }

        public Orientation Orientation
        {
            get { return Owner.Orientation; }
        }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        internal bool IsAutoLength
        {
            get { return Length.IsAuto || (Length.IsStar && Owner.SizeToContent); }
        }

        internal bool IsStarLength
        {
            get { return Length.IsStar && !Owner.SizeToContent; }
        }

        private double _measuredLength;
        internal double MeasuredLength
        {
            get { return IsVariantLength ? 0 : _measuredLength; }
            set
            {
                Debug.Assert(!IsVariantLength);
                if (_measuredLength == value)
                    return;

                _measuredLength = value;
                Owner.InvalidateOffset();
            }
        }

        internal double AvgVariantLength
        {
            get
            {
                Debug.Assert(IsVariantLength);
                LayoutXYManager.RefreshVariantLengths();
                return _measuredLength;
            }
            set
            {
                Debug.Assert(IsVariantLength);
                _measuredLength = value;
            }
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

        private LayoutXYManager LayoutXYManager
        {
            get { return Template.LayoutXYManager; }
        }

        internal int VariantLengthIndex
        {
            get
            {
                if (LayoutXYManager == null || Owner != LayoutXYManager.GridTracksMain)
                    return -1;

                return IsRepeat ? Ordinal - Owner.BlockStart.Ordinal : -1;
            }
        }

        internal bool IsVariantLength
        {
            get { return VariantLengthIndex >= 0; }
        }

        private GridSpan VariantLengthTracks
        {
            get { return LayoutXYManager.VariantLengthTracks; }
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

        private double FixBlockLength
        {
            get { return Owner.BlockEnd.EndOffset - Owner.BlockStart.StartOffset; }
        }

        private double AvgBlockLength
        {
            get { return FixBlockLength + BlockViews.AvgVariantLength; }
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
            return count == 0 ? 0 : count * FixBlockLength + BlockViews[count - 1].EndVariantLengthOffset;
        }

        internal Span GetRelativeSpan(BlockView block)
        {
            Debug.Assert(IsVariantLength && block != null);
            return GetRelativeSpan(block.Ordinal);
        }

        private Span GetRelativeSpan(int blockOrdinal)
        {
            Debug.Assert(IsVariantLength && blockOrdinal >= 0);
            var startTrack = Owner.BlockStart;

            var startOffset = StartOffset - startTrack.StartOffset;
            var endOffset = EndOffset - startTrack.StartOffset;

            var variantLengthSpan = GetVariantLengthSpan(blockOrdinal);
            startOffset += variantLengthSpan.StartOffset;
            endOffset += variantLengthSpan.EndOffset;

            return new Span(startOffset, endOffset);
        }

        private Span GetVariantLengthSpan(int blockOrdinal)
        {
            Debug.Assert(IsVariantLength);

            double startOffset, endOffset;

            var blockView = BlockViews.GetBlockView(blockOrdinal);
            if (blockView != null)
            {
                startOffset = blockView.GetVariantLengthStart(this);
                endOffset = blockView.GetVariantLengthEnd(this);
            }
            else
            {
                endOffset = 0;
                for (int i = 0; i <= VariantLengthIndex; i++)
                    endOffset += VariantLengthTracks[i].AvgVariantLength;
                startOffset = endOffset - AvgVariantLength;
            }

            return new Span(startOffset, endOffset);
        }
    }
}
