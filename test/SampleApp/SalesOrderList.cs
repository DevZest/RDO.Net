using DevZest.Data;
using DevZest.Data.Windows;
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
            .AddBinding(0, 1, new ScalarBinding<RowHeader>())
            .AddBinding(1, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.SalesOrderID).ToString() })
            .AddBinding(2, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = (p.GetValue(_.SalesOrderNumber) ?? string.Empty).ToString() })
            .AddBinding(3, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.DueDate).ToString() })
            .AddBinding(4, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.ShipDate).ToString() })
            .AddBinding(5, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.Status).ToString() })
            .AddBinding(6, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = (p.GetValue(_.PurchaseOrderNumber) ?? string.Empty).ToString() })
            .AddBinding(7, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.SubTotal).ToString() })
            .AddBinding(8, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.TaxAmt).ToString() })
            .AddBinding(9, 1, new RowBinding<TextBlock>() { OnRefresh = (v, p) => v.Text = p.GetValue(_.TotalDue).ToString() })
            .AddBinding(2, 2, 8, 2, new ScalarBinding<TextBlock>() { OnSetup = (v) =>
                {
                    v.TextAlignment = TextAlignment.Right;
                    v.Text = "Total: ";
                }
            })
            .AddBinding(9, 2, new ScalarBinding<TextBlock>() { OnRefresh = (v) => v.Text = Rows.Sum(x => x.GetValue(_.TotalDue)).ToString() });
        }
    }
}
