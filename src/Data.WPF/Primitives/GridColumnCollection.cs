using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class GridColumnCollection : GridTrackCollection<GridColumn>
    {
        internal GridColumnCollection(Template template)
            : base(template)
        {
        }

        public override Orientation Orientation
        {
            get { return Orientation.Horizontal; }
        }

        protected override GridSpan<GridColumn> GetGridSpan(GridRange gridRange)
        {
            return gridRange.ColumnSpan;
        }

        protected override bool SizeToContent
        {
            get { return Template.SizeToContentX; }
        }

        protected override double AvailableLength
        {
            get { return Template.AvailableWidth; }
        }

        public override Vector BlockDimensionVector
        {
            get { return new Vector(Template.RowRange.MeasuredWidth, 0); }
        }
    }
}
