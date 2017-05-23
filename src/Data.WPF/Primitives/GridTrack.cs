using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows.Primitives
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
            if (Orientation == layoutOrientation || Template.FlowCount != 1)
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
            get { return Owner.VariantByContainer && IsContainer ? Ordinal - Owner.ContainerStart.Ordinal : -1; }
        }

        internal bool VariantByContainer
        {
            get { return VariantByContainerIndex >= 0; }
        }
    }
}
