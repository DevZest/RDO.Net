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
        /// This field is shared by <see cref="MeasuredLength"/> and <see cref="VariantByContainerAvgLength" />, distinguished by <see cref="VariantByContainer"/>.
        /// </remarks>
        private double _measuredValue;

        internal double MeasuredLength
        {
            get { return VariantByContainer ? 0 : _measuredValue; }
            set
            {
                Debug.Assert(!VariantByContainer);
                if (_measuredValue == value)
                    return;

                _measuredValue = value;
                Owner.InvalidateOffset();
            }
        }

        internal double VariantByContainerAvgLength
        {
            get
            {
                Debug.Assert(VariantByContainer);
                LayoutXYManager.RefreshContainerLengths();
                return _measuredValue;
            }
            set
            {
                Debug.Assert(VariantByContainer);
                _measuredValue = value;
            }
        }

        /// <remarks>
        /// This field is shared by <see cref="StartOffset"/> and <see cref="VariantByContainerStartOffset" />, distinguished by <see cref="VariantByContainer"/>.
        /// </remarks>
        private double _startOffsetValue;
        internal double StartOffset
        {
            get
            {
                Debug.Assert(!VariantByContainer);
                Owner.RefreshOffset();
                return _startOffsetValue;
            }
            set
            {
                Debug.Assert(!VariantByContainer);
                _startOffsetValue = value;
            }
        }

        internal GridTrack VariantByContainerExcluded
        {
            get
            {
                var variantByContainerIndex = VariantByContainerIndex;
                if (variantByContainerIndex < 0)
                    return this;
                var ordinal = Ordinal - (variantByContainerIndex + 1);
                return ordinal == -1 ? null : Owner[ordinal];
            }
        }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal double VariantByContainerStartOffset
        {
            get
            {
                Debug.Assert(VariantByContainer);
                LayoutXYManager.RefreshContainerLengths();
                return _startOffsetValue;
            }
            set
            {
                Debug.Assert(VariantByContainer);
                _startOffsetValue = value;
            }
        }

        internal double VariantByContainerEndOffset
        {
            get { return VariantByContainerStartOffset + VariantByContainerAvgLength; }
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
            get { return Ordinal < Owner.ContainerStart.Ordinal; }
        }

        internal bool IsRepeat
        {
            get { return Ordinal >= Owner.ContainerStart.Ordinal && Ordinal <= Owner.ContainerEnd.Ordinal; }
        }

        internal bool IsTail
        {
            get { return Ordinal > Owner.ContainerEnd.Ordinal; }
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

        internal int VariantByContainerIndex
        {
            get { return Owner.VariantByContainer && IsRepeat ? Ordinal - Owner.ContainerStart.Ordinal : -1; }
        }

        internal bool VariantByContainer
        {
            get { return VariantByContainerIndex >= 0; }
        }

        private ContainerViewList ContainerViewList
        {
            get { return LayoutXYManager.ContainerViewList; }
        }

        private int MaxContainerCount
        {
            get { return ContainerViewList.MaxCount; }
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
            var delta = GetContainerViewsLength(MaxContainerCount);
            if (!Owner.VariantByContainer && MaxContainerCount > 0)
                delta -= Owner.GetMeasuredLength(Template.BlockRange);
            return new Span(StartOffset + delta, EndOffset + delta);
        }

        internal Span GetSpan(int ordinal)
        {
            Debug.Assert(Owner == LayoutXYManager.GridTracksMain);
            Debug.Assert(IsRepeat && ordinal >= 0);

            var relativeSpan = GetRelativeSpan(ordinal);
            var startOffset = (MaxFrozenHead == 0 ? 0 : Owner[MaxFrozenHead - 1].EndOffset) + GetContainerViewsLength(ordinal);
            return new Span(startOffset + relativeSpan.Start, startOffset + relativeSpan.End);
        }

        private double GetContainerViewsLength(int count)
        {
            Debug.Assert(count >= 0 && count <= MaxContainerCount);
            if (count == 0)
                return 0;

            if (!Owner.VariantByContainer)
                return Owner.GetGridSpan(Template.RowRange).MeasuredLength * count;

            var unrealized = ContainerViewList.Count == 0 ? 0 : ContainerViewList.First.ContainerOrdinal;
            if (count <= unrealized)
                return count * ContainerViewList.AvgLength;

            var realized = ContainerViewList.Count == 0 ? 0 : ContainerViewList.Last.ContainerOrdinal - ContainerViewList.First.ContainerOrdinal + 1;
            if (count <= unrealized + realized)
                return unrealized * ContainerViewList.AvgLength + GetRealizedContainersLength(count - unrealized);

            return GetRealizedContainersLength(realized) + (count - realized) * ContainerViewList.AvgLength;
        }

        private double GetRealizedContainersLength(int count)
        {
            Debug.Assert(count >= 0 && count <= ContainerViewList.Count);
            return count == 0 ? 0 : ContainerViewList[count - 1].EndOffset;
        }

        private Span GetRelativeSpan(ContainerView containerView)
        {
            Debug.Assert(containerView != null);
            return VariantByContainer ? containerView.GetReleativeSpan(this) : GetRelativeSpan();
        }

        private Span GetRelativeSpan(int ordinal)
        {
            Debug.Assert(ordinal >= 0 && ordinal < MaxContainerCount);

            if (!VariantByContainer)
                return GetRelativeSpan();

            var containerView = ContainerViewList.GetContainerView(ordinal);
            return containerView != null ? GetRelativeSpan(containerView) : new Span(VariantByContainerStartOffset, VariantByContainerEndOffset);
        }

        private Span GetRelativeSpan()
        {
            Debug.Assert(IsRepeat && !VariantByContainer);
            var originOffset = Owner.GetGridSpan(Template.RowRange).StartTrack.StartOffset;
            return new Span(StartOffset - originOffset, EndOffset - originOffset);
        }

        private Dictionary<RowPresenter, double> _availableLengths;
        internal double GetAvailableLength(RowPresenter rowPresenter)
        {
            Debug.Assert(VariantByContainer && rowPresenter != null);
            if (_availableLengths == null)
                return DefaultAvailableLength;
            double result;
            return _availableLengths.TryGetValue(rowPresenter, out result) ? result : DefaultAvailableLength;
        }

        private double DefaultAvailableLength
        {
            get
            {
                Debug.Assert(VariantByContainer);
                if (Length.IsAuto)
                    return double.PositiveInfinity;
                Debug.Assert(Length.IsAbsolute);
                return Length.Value;
            }
        }

        internal void SetAvailableLength(RowPresenter row, double value)
        {
            Debug.Assert(VariantByContainer && row != null);
            if (_availableLengths == null)
                _availableLengths = new Dictionary<RowPresenter, double>();
            _availableLengths[row] = value;
        }

        internal void ClearAvailableLength(RowPresenter row)
        {
            Debug.Assert(VariantByContainer && row != null);
            if (_availableLengths == null)
                return;
            _availableLengths.Remove(row);
        }
    }
}
