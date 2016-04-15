using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class GridTrack
    {
        internal GridTrack(Template owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
        }

        internal Template Owner { get; private set; }

        internal int Ordinal { get; private set; }

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

        internal int AutoLengthIndex { get; set; }

        internal double MeasuredLength { get; set; }

        internal double MeasuredStartOffset { get; set; }

        internal double MeasuredEndOffset
        {
            get { return MeasuredStartOffset + MeasuredLength; }
        }

        public abstract Orientation Orientation { get; }

        internal double GetMeasuredLength(BlockView blockView)
        {
            return blockView == null ? MeasuredLength : blockView.GetMeasuredLength(this);
        }

        internal bool SetMeasuredAutoLength(BlockView blockView, double value)
        {
            if (blockView == null)
            {
                var delta = value - MeasuredLength;
                Debug.Assert(delta > 0);
                MeasuredLength = value;
                if (Orientation == Orientation.Horizontal)
                    Owner.GridColumns.TotalAutoLength += delta;
                else
                    Owner.GridRows.TotalAutoLength += delta;
                return true;
            }
            else
            {
                blockView.SetMeasuredLength(this, value);
                return false;
            }
        }
    }
}
