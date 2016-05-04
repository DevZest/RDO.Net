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

        protected override double BlockDimensionLength
        {
            get { return Template.RowRange.MeasuredHeight; }
        }

        public override int FrozenHead
        {
            get { return Template.FrozenLeft; }
        }

        public override int FrozenTail
        {
            get { return Template.FrozenRight; }
        }

        protected override string FrozenHeadName
        {
            get { return nameof(Template.FrozenLeft); }
        }

        protected override string FrozenTailName
        {
            get { return nameof(Template.FrozenRight); }
        }

        public override Vector ToVector(double length, double crossLength)
        {
            return new Vector(length, crossLength);
        }
    }
}
