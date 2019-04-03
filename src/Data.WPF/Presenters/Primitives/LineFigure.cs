using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal struct LineFigure
    {
        public readonly Point StartPoint;
        public readonly Point EndPoint;

        public LineFigure(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
