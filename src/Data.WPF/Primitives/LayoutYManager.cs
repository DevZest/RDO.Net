using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class LayoutYManager : LayoutXYManager
    {
        public LayoutYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
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

        public override double ViewportX
        {
            get { return ViewportCross; }
        }

        public override double ViewportY
        {
            get { return ViewportMain; }
        }

        public override double ExtentX
        {
            get { return ExtentCross; }
        }

        public override double ExtentY
        {
            get { return ExtentMain; }
        }

        public override double ScrollOffsetX
        {
            get { return ScrollOffsetCross; }
            set { ScrollOffsetCross = value; }
        }

        public override double ScrollOffsetY
        {
            get { return ScrollOffsetMain; }
            set { ScrollOffsetMain = value; }
        }

        protected override IEnumerable<LineFigure> GetLineFiguresX(int startGridOffsetX, int endGridOffsetX, GridLinePosition position, int gridOffsetY)
        {
            return GetLineFiguresCross(startGridOffsetX, endGridOffsetX, position, gridOffsetY);
        }

        protected override IEnumerable<LineFigure> GetLineFiguresY(int startGridOffsetY, int endGridOffsetY, GridLinePosition position, int gridOffsetX)
        {
            return GetLineFiguresMain(startGridOffsetY, endGridOffsetY, position, gridOffsetX);
        }
    }
}
