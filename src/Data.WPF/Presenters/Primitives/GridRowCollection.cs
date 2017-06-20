using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
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

        public override bool SizeToContent
        {
            get { return Template.SizeToContentY; }
        }

        public override double AvailableLength
        {
            get { return Template.AvailableHeight; }
        }

        public override int FrozenHeadTracksCount
        {
            get { return Template.FrozenTop; }
        }

        public override int FrozenTailTracksCount
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

        public override Vector ToVector(double valueMain, double valueCross)
        {
            return new Vector(valueCross, valueMain);
        }
    }
}
