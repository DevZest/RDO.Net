using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class GridLineLayer : FrameworkElement
    {
        private DataView DataView
        {
            get { return TemplatedParent as DataView; }
        }

        private DataPresenter DataPresenter
        {
            get
            {
                var dataView = DataView;
                return dataView == null ? null : dataView.DataPresenter;
            }
        }

        private LayoutManager LayoutManager
        {
            get { return DataPresenter == null ? null : DataPresenter.LayoutManager; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return;

            foreach (var gridLineFigure in layoutManager.GridLineFigures)
            {
                var pen = gridLineFigure.GridLine.Pen;
                var startPoint = gridLineFigure.StartPoint;
                var endPoint = gridLineFigure.EndPoint;
                drawingContext.DrawLine(pen, startPoint, endPoint);
            }
        }
    }
}
