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

        protected override IEnumerable<GridLineFigure> GetGridLineFigures(GridLine gridLine)
        {
            return gridLine.Orientation == Orientation.Horizontal
                ? GetGridLineFiguresMain(gridLine.StartGridPoint.OffsetY, gridLine.Position, gridLine.StartGridPoint.OffsetX, gridLine.EndGridPoint.OffsetX)
                : GetGridLineFiguresCross(gridLine.StartGridPoint.OffsetX, gridLine.Position, gridLine.StartGridPoint.OffsetY, gridLine.EndGridPoint.OffsetY);
        }
    }
}
