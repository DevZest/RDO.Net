using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal struct GridLineFigure
    {
        public readonly GridLine GridLine;
        public readonly Point StartPoint;
        public readonly Point EndPoint;

        internal GridLineFigure(GridLine gridLine, LineFigure lineFigure)
            : this(gridLine, lineFigure.StartPoint, lineFigure.EndPoint)
        {
        }

        internal GridLineFigure(GridLine gridLine, Point startPoint, Point endPoint)
        {
            Debug.Assert(gridLine != null);
            GridLine = gridLine;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
