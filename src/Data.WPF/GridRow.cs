using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridRow : GridTrack, IGridRowSet
    {
        internal GridRow(GridTemplate owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        public GridLength Height
        {
            get { return Length; }
        }

        public double MinHeight
        {
            get { return MinLength; }
        }

        public double MaxHeight
        {
            get { return MaxLength; }
        }

        public double MeasuredHeight
        {
            get { return MeasuredLength; }
            internal set { MeasuredLength = value; }
        }

        GridRow IGridTrackSet<GridRow>.this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return this;
            }
        }
    }
}