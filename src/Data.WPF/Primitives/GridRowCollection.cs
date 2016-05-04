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

        public override Vector BlockDimensionVector
        {
            get { return new Vector(0, Template.RowRange.MeasuredHeight); }
        }

        protected override int FrozenHead
        {
            get { return Template.FrozenTop; }
        }

        protected override int FrozenTail
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
    }
}
