using DevZest.Data;
using System;
using System.Collections.Generic;

namespace DevZest.Windows.Data.Primitives
{
    internal sealed class LayoutXManager : ScrollableManager
    {
        public LayoutXManager(Template template, DataSet dataSet, Func<Model, Column<bool?>> where, Func<Model, ColumnSort[]> orderBy)
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
        }

        public override double VerticalOffset
        {
            get { return ScrollOffsetCross; }
        }

        protected override IEnumerable<LineFigure> GetLineFiguresX(int startGridPointX, int endGridPointX, GridPlacement? placement, int gridPointY)
        {
            return GetLineFiguresMain(startGridPointX, endGridPointX, placement, gridPointY);
        }

        protected override IEnumerable<LineFigure> GetLineFiguresY(int startGridPointY, int endGridPointY, GridPlacement? placement, int gridPointX)
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

        public override int FrozenHeadGridExtentX
        {
            get { return FrozenHeadGridExtentMain; }
        }

        public override int FrozenTailGridExtentX
        {
            get { return FrozenTailGridExtentMain; }
        }

        public override int FrozenHeadGridExtentY
        {
            get { return FrozenHeadGridExtentCross; }
        }

        public override int FrozenTailGridExtentY
        {
            get { return FrozenTailGridExtentCross; }
        }

        protected override double GetExtentXCore(int gridExtentX)
        {
            return GetExtentMain(gridExtentX);
        }

        protected sealed override double GetExtentYCore(int gridExtentY)
        {
            return GetExtentCross(gridExtentY);
        }

        protected sealed override double GetPositionXCore(int gridExtentX, GridPlacement placement)
        {
            return GetPositionMain(gridExtentX, placement);
        }

        protected sealed override double GetPositionYCore(int gridExtentY, GridPlacement placement)
        {
            return GetPositionCross(gridExtentY, placement);
        }

        public sealed override void ScrollBy(double x, double y)
        {
            InternalScrollBy(x, y);
        }

        public sealed override void ScrollToX(int gridExtent, double fraction, GridPlacement placement)
        {
            ScrollToMain(gridExtent, fraction, placement, true);
        }

        public sealed override void ScrollToY(int gridExtent, double fraction, GridPlacement placement)
        {
            ScrollToCross(gridExtent, fraction, placement);
        }

        public sealed override double ScrollableWidth
        {
            get { return Math.Max(0, ViewportWidth - FrozenHeadLengthMain - FrozenTailLengthMain); }
        }

        public sealed override double ScrollableHeight
        {
            get { return Math.Max(0, ViewportHeight - FrozenHeadLengthCross - FrozenTailLengthCross); }
        }

        public sealed override void ScrollPageUp()
        {
            ScrollBy(-ScrollableWidth, 0);
        }

        public sealed override void ScrollPageDown()
        {
            ScrollBy(ScrollableWidth, 0);
        }
    }
}
