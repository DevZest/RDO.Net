using DevZest.Data;
using System;
using System.Collections.Generic;

namespace DevZest.Windows.Data.Primitives
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
        }

        public override double VerticalOffset
        {
            get { return ScrollOffsetMain; }
        }

        protected override IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPlacement? placement, int gridPointY)
        {
            return GetLineFiguresCross(startGridPointX, endGridPointX, placement, gridPointY);
        }

        protected override IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPlacement? placement, int gridPointX)
        {
            return GetLineFiguresMain(startGridPointY, endGridPointY, placement, gridPointX);
        }

        public override int MaxGridExtentX
        {
            get { return MaxGridExtentCross; }
        }

        public override int MaxGridExtentY
        {
            get { return MaxGridExtentMain; }
        }

        protected sealed override double GetExtentXCore(int gridExtentX)
        {
            return GetExtentCross(gridExtentX);
        }

        protected sealed override double GetExtentYCore(int gridExtentY)
        {
            return GetExtentMain(gridExtentY);
        }

        protected sealed override double GetPositionXCore(int gridExtentX, GridPlacement placement)
        {
            return GetPositionCross(gridExtentX, placement);
        }

        protected sealed override double GetPositionYCore(int gridExtentY, GridPlacement placement)
        {
            return GetPositionMain(gridExtentY, placement);
        }

        public sealed override void ScrollBy(double x, double y)
        {
            InternalScrollBy(y, x);
        }
    }
}
