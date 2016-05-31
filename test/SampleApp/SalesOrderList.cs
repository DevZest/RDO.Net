using DevZest.Data;
using DevZest.Data.Windows;
using DevZest.Data.Windows.Factories;
using DevZest.Data.Windows.Primitives;
using DevZest.Samples.AdventureWorksLT;
using System.Windows.Controls;
using System.Windows.Media;

namespace SampleApp
{
    public class SalesOrderList : DataView
    {
        private Pen _gridLinePen = new Pen(Brushes.LightGray, 1);

        public void Show(DataSet<SalesOrder> salesOrders)
        {
            Show(salesOrders, (builder, _) =>
            {
                builder.GridColumns("20", "50", "70", "Auto", "Auto", "Auto", "Auto")
                    .GridRows("Auto", "Auto")
                    .Layout(Orientation.Vertical)
                    .GridLineX(new GridPoint(0, 1), 7, _gridLinePen, GridLinePosition.NextTrack)
                    .GridLineX(new GridPoint(0, 2), 7, _gridLinePen, GridLinePosition.PreviousTrack)
                    .GridLineY(new GridPoint(1, 0), 2, _gridLinePen)
                    .GridLineY(new GridPoint(2, 0), 2, _gridLinePen)
                    .RowHeader().At(0, 1)
                    .TextBlock(_.SalesOrderID).At(1, 1)
                    .TextBlock(_.SalesOrderNumber).At(2, 1)
                    .TextBlock(_.Status).At(3, 1)
                    .TextBlock(_.DueDate).At(4, 1)
                    .TextBlock(_.DueDate).At(5, 1)
                    .TextBlock(_.ShipDate).At(6, 1);
            });
        }
    }
}
