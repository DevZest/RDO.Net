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

        private LayoutManagerXY LayoutManagerXY
        {
            get { return Template.LayoutManager as LayoutManagerXY; }
        }

        internal int Ordinal { get; private set; }

        public Orientation Orientation
        {
            get { return Owner.Orientation; }
        }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        internal bool IsAutoLength(bool sizeToContent)
        {
            return Length.IsAuto || (Length.IsStar && sizeToContent);
        }

        internal bool IsStarLength(bool sizeToContent)
        {
            return Length.IsStar && !sizeToContent;
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

        internal double TotalVariantAutoLength
        {
            get
            {
                Debug.Assert(IsVariantAutoLength);
                LayoutManagerXY.RefreshVariantAutoLengths();
                return _measuredLength;
            }
        }

        internal void SetTotalVariantAutoLength(double value)
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
    }
}
