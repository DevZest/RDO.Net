using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevZest.Data.Presenters;

namespace SampleApp
{
    public class SalesOrderList : DataPresenter<SalesOrder>
    {
        private readonly Pen _frozenLine;
        
        public SalesOrderList()
        {
            _frozenLine = new Pen(Brushes.Black, 1);
            _frozenLine.Freeze();
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("20", "50", "70", "150", "150", "60", "150", "100", "100", "100")
            .GridRows("20", "Auto", "20")
            .Layout(Orientation.Vertical)
            .WithFrozenLeft(2).WithFrozenRight(1).WithFrozenTop(1).WithFrozenBottom(1).WithStretches(1)
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
            .GridLineY(new GridPoint(9, 0), 3, _frozenLine)
            .GridLineY(new GridPoint(10, 0), 3)
            .AddBinding(0, 1, _.AsRowHeader())
            .AddBinding(1, 1, _.SalesOrderID.AsTextBlock())
            .AddBinding(2, 1, _.SalesOrderNumber.AsTextBlock())
            .AddBinding(3, 1, _.DueDate.AsTextBlock())
            .AddBinding(4, 1, _.ShipDate.AsTextBlock())
            .AddBinding(5, 1, _.Status.AsTextBlock())
            .AddBinding(6, 1, _.PurchaseOrderNumber.AsTextBlock())
            .AddBinding(7, 1, _.SubTotal.AsTextBlock())
            .AddBinding(8, 1, _.TaxAmt.AsTextBlock())
            .AddBinding(9, 1, _.TotalDue.AsTextBlock())
            .AddBinding(2, 2, 8, 2, new ScalarBinding<TextBlock>(onSetup: (v, sp) =>
                {
                    v.TextAlignment = TextAlignment.Right;
                    v.Text = "Total: ";
                }, onRefresh: null, onCleanup: null))
            .AddBinding(9, 2, new ScalarBinding<TextBlock>(onRefresh : (v) => { v.Text = Rows.Sum(x => x.GetValue(_.TotalDue)).ToString(); }));
        }
    }
}
