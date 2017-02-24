using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutXManager : ScrollableManager
    {
        public LayoutXManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
        }

        internal override IGridTrackCollection GridTracksMain
        {
            get { return Template.InternalGridColumns; }
        }

        internal override IGridTrackCollection GridTracksCross
        {
            get { return Template.InternalGridRows; }
        }

        public override double ViewportWidth
        {
            get { return ViewportMain; }
        }

        public override double ViewportHeight
        {
            get { return ViewportCross; }
        }

        public override double ExtentWidth
        {
            get { return ExtentMain; }
        }

        public override double ExtentHeight
        {
            get { return ExtentCross; }
        }

        public override double HorizontalOffset
        {
            get { return ScrollOffsetMain; }
            set { ScrollOffsetMain = value; }
        }

        public override double VerticalOffset
        {
            get { return ScrollOffsetCross; }
            set { ScrollOffsetCross = value; }
        }

        protected override IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPointPlacement placement, int gridPointY)
        {
            return GetLineFiguresMain(startGridPointX, endGridPointX, placement, gridPointY);
        }

        protected override IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPointPlacement placement, int gridPointX)
        {
            return GetLineFiguresCross(startGridPointY, endGridPointY, placement, gridPointX);
        }

        public override int MaxGridExtentX
        {
            get { return MaxGridExtentMain; }
        }

        public override int MaxGridExtentY
        {
            get { return MaxGridExtentCross; }
        }

        protected override double GetExtentXCore(int gridExtentX)
        {
            return GetExtentMain(gridExtentX);
        }

        protected sealed override double GetExtentYCore(int gridExtentY)
        {
            return GetExtentCross(gridExtentY);
        }

        protected sealed override double GetPositionXCore(int gridExtentX, GridPointPlacement placement)
        {
            return GetPositionMain(gridExtentX, placement);
        }

        protected sealed override double GetPositionYCore(int gridExtentY, GridPointPlacement placement)
        {
            return GetPositionCross(gridExtentY, placement);
        }
    }
}
