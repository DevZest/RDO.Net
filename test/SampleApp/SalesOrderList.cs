using DevZest.Data;
using DevZest.Data.Windows;
using DevZest.Data.Windows.Controls;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;

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
            .FrozenLeft(2).FrozenRight(1).FrozenTop(1).FrozenBottom(1).Stretch(1)
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
            .ScalarItem<RowHeader>()
            .At(0, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.SalesOrderID).ToString())
            .At(1, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = (p.GetValue(_.SalesOrderNumber) ?? string.Empty).ToString())
            .At(2, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.DueDate).ToString())
            .At(3, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.ShipDate).ToString())
            .At(4, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.Status).ToString())
            .At(5, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = (p.GetValue(_.PurchaseOrderNumber) ?? string.Empty).ToString())
            .At(6, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.SubTotal).ToString())
            .At(7, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.TaxAmt).ToString())
            .At(8, 1)
            .RowItem<TextBlock>()
                .OnRefresh((v, p) => v.Text = p.GetValue(_.TotalDue).ToString())
            .At(9, 1)
            .ScalarItem<TextBlock>()
                .OnMount((v) =>
                {
                    v.TextAlignment = TextAlignment.Right;
                    v.Text = "Total: ";
                })
            .At(2, 2, 8, 2)
            .ScalarItem<TextBlock>()
                .OnRefresh((v) =>
                {
                    v.Text = Rows.Sum(x => x.GetValue(_.TotalDue)).ToString();
                })
            .At(9, 2);
        }
    }
}
