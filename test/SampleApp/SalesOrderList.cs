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
            builder.GridColumns("20", "50", "70", "60", "150", "150")
                .GridRows("1", "Auto")
                .Layout(Orientation.Vertical)
                .FrozenLeft(2)
                .GridLineX(new GridPoint(0, 1), 6)
                .GridLineX(new GridPoint(0, 2), 6)
                .GridLineY(new GridPoint(1, 0), 2)
                .GridLineY(new GridPoint(2, 0), 2, _frozenLine)
                .GridLineY(new GridPoint(3, 0), 2)
                .GridLineY(new GridPoint(4, 0), 2)
                .GridLineY(new GridPoint(5, 0), 2)
                .GridLineY(new GridPoint(6, 0), 2)
                .RowHeader().At(0, 1)
                .TextBlock(_.SalesOrderID).At(1, 1)
                .TextBlock(_.SalesOrderNumber).At(2, 1)
                .TextBlock(_.Status).At(3, 1)
                .TextBlock(_.DueDate).At(4, 1)
                .TextBlock(_.ShipDate).At(5, 1);
        }
    }
}
