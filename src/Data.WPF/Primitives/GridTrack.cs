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

        /// <remarks>
        /// This field is shared by <see cref="MeasuredLength"/> and <see cref="VariantByBlockAvgLength" />, distinguished by <see cref="VariantByBlock"/>.
        /// </remarks>
        private double _measuredValue;

        internal double MeasuredLength
        {
            get { return VariantByBlock ? 0 : _measuredValue; }
            set
            {
                Debug.Assert(!VariantByBlock);
                if (_measuredValue == value)
                    return;

                _measuredValue = value;
                Owner.InvalidateOffset();
            }
        }

        internal double VariantByBlockAvgLength
        {
            get
            {
                Debug.Assert(VariantByBlock);
                LayoutXYManager.RefreshBlockLengths();
                return _measuredValue;
            }
            set
            {
                Debug.Assert(VariantByBlock);
                _measuredValue = value;
            }
        }

        /// <remarks>
        /// This field is shared by <see cref="StartOffset"/> and <see cref="VariantByBlockStartOffset" />, distinguished by <see cref="VariantByBlock"/>.
        /// </remarks>
        private double _startOffsetValue;
        internal double StartOffset
        {
            get
            {
                Debug.Assert(!VariantByBlock);
                Owner.RefreshOffset();
                return _startOffsetValue;
            }
            set
            {
                Debug.Assert(!VariantByBlock);
                _startOffsetValue = value;
            }
        }

        internal GridTrack VariantByBlockExcluded
        {
            get
            {
                var variantByBlockIndex = VariantByBlockIndex;
                if (variantByBlockIndex < 0)
                    return this;
                var ordinal = Ordinal - (variantByBlockIndex + 1);
                return ordinal == -1 ? null : Owner[ordinal];
            }
        }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal double VariantByBlockStartOffset
        {
            get
            {
                Debug.Assert(VariantByBlock);
                LayoutXYManager.RefreshBlockLengths();
                return _startOffsetValue;
            }
            set
            {
                Debug.Assert(VariantByBlock);
                _startOffsetValue = value;
            }
        }

        internal double VariantByBlockEndOffset
        {
            get { return VariantByBlockStartOffset + VariantByBlockAvgLength; }
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

        internal bool IsFrozenHead
        {
            get { return Ordinal < Owner.FrozenHead; }
        }

        internal bool IsFrozenTail
        {
            get { return Ordinal >= Owner.Count - Owner.FrozenTail; }
        }

        private LayoutXYManager LayoutXYManager
        {
            get { return Template.LayoutXYManager; }
        }

        internal int VariantByBlockIndex
        {
            get { return Owner.VariantByBlock && IsRepeat ? Ordinal - Owner.BlockStart.Ordinal : -1; }
        }

        internal bool VariantByBlock
        {
            get { return VariantByBlockIndex >= 0; }
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
            var startOffset = (MaxFrozenHead == 0 ? 0 : Owner[MaxFrozenHead - 1].EndOffset) + GetBlocksLength(blockOrdinal);
            return new Span(startOffset + relativeSpan.Start, startOffset + relativeSpan.End);
        }

        private double GetBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= MaxBlockCount);
            if (count == 0)
                return 0;

            if (!Owner.VariantByBlock)
                return Owner.GetGridSpan(Template.RowRange).MeasuredLength * count;

            var unrealized = BlockViews.Count == 0 ? 0 : BlockViews.First.Ordinal;
            if (count <= unrealized)
                return count * BlockViews.AvgLength;

            var realized = BlockViews.Count == 0 ? 0 : BlockViews.Last.Ordinal - BlockViews.First.Ordinal + 1;
            if (count <= unrealized + realized)
                return unrealized * BlockViews.AvgLength + GetRealizedBlocksLength(count - unrealized);

            return GetRealizedBlocksLength(realized) + (count - realized) * BlockViews.AvgLength;
        }

        private double GetRealizedBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= BlockViews.Count);
            return count == 0 ? 0 : BlockViews[count - 1].EndOffset;
        }

        private Span GetRelativeSpan(BlockView block)
        {
            Debug.Assert(block != null);
            return VariantByBlock ? block.GetReleativeSpan(this) : GetRelativeSpan();
        }

        private Span GetRelativeSpan(int blockOrdinal)
        {
            Debug.Assert(blockOrdinal >= 0 && blockOrdinal < MaxBlockCount);

            if (!VariantByBlock)
                return GetRelativeSpan();

            var block = BlockViews.GetBlockView(blockOrdinal);
            return block != null ? GetRelativeSpan(block) : new Span(VariantByBlockStartOffset, VariantByBlockEndOffset);
        }

        private Span GetRelativeSpan()
        {
            Debug.Assert(IsRepeat && !VariantByBlock);
            var originOffset = Owner.GetGridSpan(Template.RowRange).StartTrack.StartOffset;
            return new Span(StartOffset - originOffset, EndOffset - originOffset);
        }

        private Dictionary<RowPresenter, double> _availableLengths;
        internal double GetAvailableLength(RowPresenter rowPresenter)
        {
            Debug.Assert(VariantByBlock && rowPresenter != null);
            if (_availableLengths == null)
                return DefaultAvailableLength;
            double result;
            return _availableLengths.TryGetValue(rowPresenter, out result) ? result : DefaultAvailableLength;
        }

        private double DefaultAvailableLength
        {
            get
            {
                Debug.Assert(VariantByBlock);
                if (Length.IsAuto)
                    return double.PositiveInfinity;
                Debug.Assert(Length.IsAbsolute);
                return Length.Value;
            }
        }

        internal void SetAvailableLength(RowPresenter row, double value)
        {
            Debug.Assert(VariantByBlock && row != null);
            if (_availableLengths == null)
                _availableLengths = new Dictionary<RowPresenter, double>();
            _availableLengths[row] = value;
        }

        internal void ClearAvailableLength(RowPresenter row)
        {
            Debug.Assert(VariantByBlock && row != null);
            if (_availableLengths == null)
                return;
            _availableLengths.Remove(row);
        }
    }
}
