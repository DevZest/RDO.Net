using DevZest.Data;
using DevZest.Data.Windows;
using DevZest.Data.Windows.Factories;
using DevZest.Samples.AdventureWorksLT;
using System.Windows.Controls;
using System.Windows.Media;

namespace SampleApp
{
    public class SalesOrderList : DataView
    {
        private readonly Pen _frozenLine;
        
        public SalesOrderList()
        {
            _frozenLine = new Pen(Brushes.Black, 1);
            _frozenLine.Freeze();
        }

        public void Show(DataSet<SalesOrder> salesOrders)
        {
            Show(salesOrders, BuildTemplate);
        }

        private void BuildTemplate(TemplateBuilder builder, SalesOrder _)
        {
            builder.GridColumns("20", "50", "70", "150", "150", "60", "150", "100", "100", "100")
                .GridRows("20", "Auto", "20")
                .Layout(Orientation.Vertical)
                .FrozenLeft(2).FrozenRight(1).FrozenTop(1).FrozenBottom(1)
                .GridLineX(new GridPoint(0, 1), 10)
                .GridLineX(new GridPoint(0, 2), 10)
                .GridLineY(new GridPoint(1, 0), 2)
                .GridLineY(new GridPoint(2, 0), 2, _frozenLine)
                .GridLineY(new GridPoint(3, 0), 2)
                .GridLineY(new GridPoint(4, 0), 2)
                .GridLineY(new GridPoint(5, 0), 2)
                .GridLineY(new GridPoint(6, 0), 2)
                .GridLineY(new GridPoint(7, 0), 2)
                .GridLineY(new GridPoint(8, 0), 2)
                .GridLineY(new GridPoint(9, 0), 2, _frozenLine)
                .GridLineY(new GridPoint(10, 0), 2)
                .RowHeader().At(0, 1)
                .TextBlock(_.SalesOrderID).At(1, 1)
                .TextBlock(_.SalesOrderNumber).At(2, 1)
                .TextBlock(_.DueDate).At(3, 1)
                .TextBlock(_.ShipDate).At(4, 1)
                .TextBlock(_.Status).At(5, 1)
                .TextBlock(_.PurchaseOrderNumber).At(6, 1)
                .TextBlock(_.SubTotal).At(7, 1)
                .TextBlock(_.TaxAmt).At(8, 1)
                .TextBlock(_.TotalDue).At(9, 1);
        }
    }
}
