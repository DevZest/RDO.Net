using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public sealed class GridRow : GridTrack, IGridRowSet
    {
        internal GridRow(Template owner, int ordinal, GridLengthParser.Result result)
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

        GridRow IGridTrackSet<GridRow>.this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return this;
            }
        }

        public override Orientation Orientation
        {
            get { return Orientation.Vertical; }
        }
    }
}