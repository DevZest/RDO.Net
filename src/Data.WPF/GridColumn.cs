using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridColumn : GridTrack, IGridColumnSet
    {
        internal GridColumn(GridTemplate owner, int ordinal, GridLengthParser.Result result)
            : base(owner, ordinal, result)
        {
        }

        public GridLength Width
        {
            get { return Length; }
        }

        public double MinWidth
        {
            get { return MinLength; }
        }

        public double MaxWidth
        {
            get { return MaxWidth; }
        }

        public double MeasuredWidth
        {
            get { return MeasuredLength; }
            internal set { MeasuredLength = value; }
        }

        GridColumn IGridTrackSet<GridColumn>.this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return this;
            }
        }
    }
}
