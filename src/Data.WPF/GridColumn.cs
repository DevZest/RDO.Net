using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public sealed class GridColumn : GridTrack, IGridColumnSet
    {
        internal GridColumn(Template owner, int ordinal, GridLengthParser.Result result)
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
            get { return MaxLength; }
        }

        GridColumn IGridTrackSet<GridColumn>.this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return this;
            }
        }

        public override Orientation Orientation
        {
            get { return Orientation.Horizontal; }
        }
    }
}
