using System;
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

        protected override bool SizeToContent
        {
            get { return Template.SizeToContentY; }
        }

        protected override double AvailableLength
        {
            get { return Template.AvailableHeight; }
        }
    }
}
