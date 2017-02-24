using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutYManager : ScrollableManager
    {
        public LayoutYManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
        }

        internal override IGridTrackCollection GridTracksMain
        {
            get { return Template.InternalGridRows; }
        }

        internal override IGridTrackCollection GridTracksCross
        {
            get { return Template.InternalGridColumns; }
        }

        public override double ViewportWidth
        {
            get { return ViewportCross; }
        }

        public override double ViewportHeight
        {
            get { return ViewportMain; }
        }

        public override double ExtentWidth
        {
            get { return ExtentCross; }
        }

        public override double ExtentHeight
        {
            get { return ExtentMain; }
        }

        public override double HorizontalOffset
        {
            get { return ScrollOffsetCross; }
            set { ScrollOffsetCross = value; }
        }

        public override double VerticalOffset
        {
            get { return ScrollOffsetMain; }
            set { ScrollOffsetMain = value; }
        }

        protected override IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPointPlacement placement, int gridPointY)
        {
            return GetLineFiguresCross(startGridPointX, endGridPointX, placement, gridPointY);
        }

        protected override IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPointPlacement placement, int gridPointX)
        {
            return GetLineFiguresMain(startGridPointY, endGridPointY, placement, gridPointX);
        }
    }
}
