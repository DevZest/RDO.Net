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
            get { return WithinBlock ? 0 : _measuredLength; }
            set
            {
                Debug.Assert(!WithinBlock);
                if (_measuredLength == value)
                    return;

                _measuredLength = value;
                Owner.InvalidateOffset();
            }
        }

        internal double AvgLength
        {
            get
            {
                Debug.Assert(WithinBlock);
                LayoutXYManager.RefreshBlockLengths();
                return _measuredLength;
            }
            set
            {
                Debug.Assert(WithinBlock);
                _measuredLength = value;
            }
        }

        private double AvgLengthStartOffset
        {
            get
            {
                LayoutXYManager.RefreshBlockLengths();
                return LayoutXYManager.GetAvgLengthStartOffset(this);
            }
        }

        private double AvgLengthEndOffset
        {
            get
            {
                LayoutXYManager.RefreshBlockLengths();
                return LayoutXYManager.GetAvgLengthEndOffset(this);
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

        internal int WithinBlockIndex
        {
            get
            {
                if (LayoutXYManager == null || Owner != LayoutXYManager.GridTracksMain)
                    return -1;

                return IsRepeat ? Ordinal - Owner.BlockStart.Ordinal : -1;
            }
        }

        internal bool WithinBlock
        {
            get { return WithinBlockIndex >= 0; }
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

        private double AvgBlockLength
        {
            get { return BlockViews.AvgLength; }
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
            return count == 0 ? 0 : BlockViews[count - 1].EndOffset;
        }

        internal Span GetRelativeSpan(BlockView block)
        {
            Debug.Assert(WithinBlock && block != null);
            return new Span(block.GetRelativeStartOffset(this), block.GetRelativeEndOffset(this));
        }

        private Span GetRelativeSpan(int blockOrdinal)
        {
            Debug.Assert(WithinBlock && blockOrdinal >= 0 && blockOrdinal < MaxBlockCount);

            var block = BlockViews.GetBlockView(blockOrdinal);
            return block != null ? GetRelativeSpan(block) : new Span(AvgLengthStartOffset, AvgLengthEndOffset);
        }
    }
}
