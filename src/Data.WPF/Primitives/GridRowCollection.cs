using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class GridRowCollection : GridTrackCollection<GridRow>
    {
        internal GridRowCollection(Template template)
            : base(template)
        {
        }

        public override Orientation Orientation
        {
            get { return Orientation.Vertical; }
        }

        protected override GridSpan<GridRow> GetGridSpan(GridRange gridRange)
        {
            return gridRange.RowSpan;
        }

        protected override bool SizeToContent
        {
            get { return Template.SizeToContentY; }
        }

        protected override double AvailableLength
        {
            get { return Template.AvailableHeight; }
        }

        protected override double BlockDimensionLength
        {
            get { return Template.RowRange.MeasuredWidth; }
        }

        public override int FrozenHead
        {
            get { return Template.FrozenTop; }
        }

        public override int FrozenTail
        {
            get { return Template.FrozenBottom; }
        }

        protected override string FrozenHeadName
        {
            get { return nameof(Template.FrozenTop); }
        }

        protected override string FrozenTailName
        {
            get { return nameof(Template.FrozenBottom); }
        }

        public override Vector ToVector(double length, double crossLength)
        {
            return new Vector(crossLength, length);
        }
    }
}
