using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutXManager : LayoutXYManager
    {
        public LayoutXManager(Template template, DataSet dataSet)
            : base(template, dataSet)
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

        public override double ViewportX
        {
            get { return ViewportMain; }
        }

        public override double ViewportY
        {
            get { return ViewportCross; }
        }

        public override double ExtentX
        {
            get { return ExtentMain; }
        }

        public override double ExtentY
        {
            get { return ExtentCross; }
        }

        public override double ScrollOffsetX
        {
            get { return ScrollOffsetMain; }
            set { ScrollOffsetMain = value; }
        }

        public override double ScrollOffsetY
        {
            get { return ScrollOffsetCross; }
            set { ScrollOffsetCross = value; }
        }

        protected override IEnumerable<GridLineFigure> GetGridLineFiguresX(int startGridOffsetX, int endGridOffsetX, GridLinePosition position, int gridOffsetY)
        {
            return GetGridLineFiguresMain(startGridOffsetX, endGridOffsetX, position, gridOffsetY);
        }

        protected override IEnumerable<GridLineFigure> GetGridLineFiguresY(int startGridOffsetY, int endGridOffsetY, GridLinePosition position, int gridOffsetX)
        {
            return GetGridLineFiguresCross(startGridOffsetY, endGridOffsetY, position, gridOffsetX);
        }
    }
}
