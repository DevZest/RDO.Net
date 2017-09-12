using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
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

        public override bool SizeToContent
        {
            get { return Template.SizeToContentX; }
        }

        public override double AvailableLength
        {
            get { return Template.AvailableWidth; }
        }

        public override int FrozenHeadTracksCount
        {
            get { return Template.FrozenLeft; }
        }

        public override int FrozenTailTracksCount
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

        public override Vector ToVector(double valueMain, double valueCross)
        {
            return new Vector(valueMain, valueCross);
        }

        public override void OnResized(GridTrack gridTrack, GridLength oldValue)
        {
            base.OnResized(gridTrack, oldValue);
            Template.InvalidateGridColumns();
        }
    }
}
