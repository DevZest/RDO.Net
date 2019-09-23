using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Base class of <see cref="GridColumn"/> and <see cref="GridRow"/>.
    /// </summary>
    public abstract class GridTrack
    {
        internal GridTrack(IGridTrackCollection owner, int ordinal, GridLengthParser.Result result)
        {
            _owner = owner;
            _ordinal = ordinal;
            _length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
        }

        private readonly IGridTrackCollection _owner;
        internal IGridTrackCollection Owner
        {
            get { return _owner; }
        }

        internal Template Template
        {
            get { return Owner.Template; }
        }

        private readonly int _ordinal;
        internal int Ordinal
        {
            get { return _ordinal; }
        }

        /// <summary>
        /// Gets the orientation.
        /// </summary>
        public Orientation Orientation
        {
            get { return Owner.Orientation; }
        }

        private GridLength _length;
        /// <summary>
        /// Gets the grid length.
        /// </summary>
        public GridLength Length
        {
            get { return _length; }
        }

        private ScrollableManager ScrollableManager
        {
            get { return Owner?.ScrollableManager; }
        }

        private ScrollableManager Resizable
        {
            get
            {
                var result = ScrollableManager;
                if (result == null)
                    return result;

                return result.GridTracksMain == Owner && IsContainer ? result : null;
            }
        }

        /// <summary>
        /// Resizes this grid track.
        /// </summary>
        /// <param name="value">The new length.</param>
        public void Resize(GridLength value)
        {
            if (Orientation == Template?.Orientation && value.IsStar)
                throw new ArgumentException(DiagnosticMessages.GridTrack_Resize_InvalidStarLength(Orientation), nameof(value));

            var oldValue = _length;
            if (oldValue != value)
            {
                _length = value;
                if (!VariantByContainer)
                    SetMeasuredLength(0);
                Owner.OnResized(this, oldValue);
            }
            Resizable?.OnResized(this);
            LayoutManager?.InvalidateMeasure();
        }

        private LayoutManager LayoutManager
        {
            get { return Template.LayoutManager; }
        }

        /// <summary>
        /// Gets the minimum length of this grid track.
        /// </summary>
        public double MinLength { get; private set; }

        /// <summary>
        /// Gets the maximum length of this grid track.
        /// </summary>
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

        /// <summary>
        /// Gets the measured length.
        /// </summary>
        public double MeasuredLength
        {
            get { return VariantByContainer ? double.NaN : _measuredValue; }
        }

        internal double SetMeasuredLength(double value)
        {
            Debug.Assert(!VariantByContainer);
            if (!IsStarLength)
            {
                value = Math.Max(MinLength, value);
                value = Math.Min(MaxLength, value);
            }

            if (_measuredValue == value)
                return 0;

            var oldValue = _measuredValue;
            _measuredValue = value;
            Owner.InvalidateOffset();
            return value - oldValue;
        }

        internal double VariantByContainerAvgLength
        {
            get
            {
                Debug.Assert(VariantByContainer);
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
                return _startOffsetValue;
            }
            set
            {
                Debug.Assert(VariantByContainer);
                _startOffsetValue = value;
            }
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
            if (Orientation == layoutOrientation || Template.FlowRepeatCount != 1)
                throw new InvalidOperationException(InvalidStarLengthMessage);
        }

        internal abstract string InvalidStarLengthMessage { get; }

        private void VerifyAutoLength()
        {
            Debug.Assert(Length.IsAuto);

            if (Template.Flowable(Orientation))
                throw new InvalidOperationException(InvalidAutoLengthMessage);
        }

        internal abstract string InvalidAutoLengthMessage { get; }

        internal bool IsHead
        {
            get { return Ordinal < Owner.ContainerStart.Ordinal; }
        }

        internal bool IsContainer
        {
            get { return Ordinal >= Owner.ContainerStart.Ordinal && Ordinal <= Owner.ContainerEnd.Ordinal; }
        }

        internal bool IsRow
        {
            get { return Ordinal >= Owner.RowStart.Ordinal && Ordinal <= Owner.RowEnd.Ordinal; }
        }

        internal bool IsTail
        {
            get { return Ordinal > Owner.ContainerEnd.Ordinal; }
        }

        internal bool IsFrozenHead
        {
            get { return Ordinal < Owner.FrozenHeadTracksCount; }
        }

        internal bool IsFrozenTail
        {
            get { return Ordinal >= Owner.Count - Owner.FrozenTailTracksCount; }
        }

        internal int VariantByContainerIndex
        {
            get { return VariantByContainer ? Ordinal - Owner.ContainerStart.Ordinal : -1; }
        }

        internal bool VariantByContainer
        {
            get { return Owner.VariantByContainer && IsContainer; }
        }

        internal int ContainerIndex
        {
            get { return IsContainer ? Ordinal - Owner.ContainerStart.Ordinal : -1; }
        }
    }
}
