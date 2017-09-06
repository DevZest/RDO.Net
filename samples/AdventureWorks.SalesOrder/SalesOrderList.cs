using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System.Globalization;
using DevZest.Data.Views;

namespace AdventureWorks.SalesOrders
{
    public class SalesOrderList : DataPresenter<SalesOrder>
    {
        public static readonly StyleKey CheckBoxStyleKey = new StyleKey(typeof(SalesOrderList));
        public static readonly StyleKey LeftAlignedTextBlockStyleKey = new StyleKey(typeof(SalesOrderList));
        public static readonly StyleKey RightAlignedTextBlockStyleKey = new StyleKey(typeof(SalesOrderList));

        private readonly Pen _frozenLine;
        
        public SalesOrderList()
        {
            _frozenLine = new Pen(Brushes.Black, 1);
            _frozenLine.Freeze();
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("20", "50", "70", "70", "75", "60", "100", "70", "70", "70")
            .GridRows("Auto", "Auto", "20")
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
            .AddBinding(0, 0, this.AsSelectionCheckBox().WithStyle(CheckBoxStyleKey))
            .AddBinding(0, 1, _.AsSelectionCheckBox().WithStyle(CheckBoxStyleKey))
            .AddBinding(1, 0, _.SalesOrderID.AsColumnHeader("ID"))
            .AddBinding(1, 1, _.SalesOrderID.AsTextBlock().WithStyle(LeftAlignedTextBlockStyleKey))
            .AddBinding(2, 0, _.SalesOrderNumber.AsColumnHeader("Number"))
            .AddBinding(2, 1, _.SalesOrderNumber.AsTextBlock().WithStyle(LeftAlignedTextBlockStyleKey))
            .AddBinding(3, 0, _.DueDate.AsColumnHeader("Due Date"))
            .AddBinding(3, 1, _.DueDate.AsTextBlock("{0:d}").WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(4, 0, _.ShipDate.AsColumnHeader("Ship Date"))
            .AddBinding(4, 1, _.ShipDate.AsTextBlock("{0:d}").WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(5, 0, _.Status.AsColumnHeader("Status"))
            .AddBinding(5, 1, _.Status.AsTextBlock().WithStyle(LeftAlignedTextBlockStyleKey))
            .AddBinding(6, 0, _.PurchaseOrderNumber.AsColumnHeader("PO Number"))
            .AddBinding(6, 1, _.PurchaseOrderNumber.AsTextBlock().WithStyle(LeftAlignedTextBlockStyleKey))
            .AddBinding(7, 0, _.SubTotal.AsColumnHeader("Sub Total"))
            .AddBinding(7, 1, _.SubTotal.AsTextBlock("{0:C}").WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(8, 0, _.TaxAmt.AsColumnHeader("Tax Amt"))
            .AddBinding(8, 1, _.TaxAmt.AsTextBlock("{0:C}").WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(9, 0, _.TaxAmt.AsColumnHeader("Total Due"))
            .AddBinding(9, 1, _.TotalDue.AsTextBlock("{0:C}").WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(2, 2, 8, 2, new ScalarBinding<TextBlock>(onSetup: (v, p) =>
                {
                    v.Text = "Total: ";
                }, onRefresh: null, onCleanup: null).WithStyle(RightAlignedTextBlockStyleKey))
            .AddBinding(9, 2, new ScalarBinding<TextBlock>(onRefresh : (v) => v.Text = string.Format("{0:C}", Rows.Sum(x => x.GetValue(_.TotalDue)))).WithStyle(RightAlignedTextBlockStyleKey));
        }

        protected override void OnSetup(RowView rowView)
        {
            var index = rowView.RowPresenter.Index;
            if (index % 2 == 1)
                rowView.Background = Brushes.LightGray;
            else
                rowView.Background = Brushes.White;
        }
    }
}
