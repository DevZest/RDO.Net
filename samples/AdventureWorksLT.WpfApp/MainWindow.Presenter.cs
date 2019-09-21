using DevZest.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using DevZest.Data.Views;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class MainWindow
    {
        public static class Styles
        {
            public static readonly StyleId CheckBox = new StyleId(typeof(MainWindow));
            public static readonly StyleId LeftAlignedTextBlock = new StyleId(typeof(MainWindow));
            public static readonly StyleId RightAlignedTextBlock = new StyleId(typeof(MainWindow));
            public static readonly StyleId Label = new StyleId(typeof(MainWindow));
        }

        private class Presenter : DataPresenter<SalesOrderHeader>, ColumnHeader.ISortService
        {
            private readonly Pen _frozenLine;

            public Presenter()
            {
                _frozenLine = new Pen(Brushes.Black, 1);
                _frozenLine.Freeze();
            }

            private decimal? CalcTotalAmt()
            {
                return Rows.Sum(x => x.GetValue(_.TotalDue));
            }

            private Func<decimal?> CalcTotalAmtFunc
            {
                get { return CalcTotalAmt; }
            }

            private string _searchText;
            public string SearchText
            {
                get { return _searchText; }
                set
                {
                    _searchText = value;
                    RefreshAsync();
                }
            }

            private IReadOnlyList<IColumnComparer> _orderBy = new IColumnComparer[] { };
            IReadOnlyList<IColumnComparer> ColumnHeader.ISortService.OrderBy
            {
                get { return _orderBy; }
                set
                {
                    _orderBy = value;
                    RefreshAsync();
                }
            }

            private Task<DataSet<SalesOrderHeader>> LoadDataAsync(CancellationToken ct)
            {
                return App.ExecuteAsync(db => db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct));
            }

            public void ShowAsync(DataView dataView)
            {
                ShowAsync(dataView, LoadDataAsync);
            }

            public Task RefreshAsync()
            {
                return RefreshAsync(LoadDataAsync);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridColumns("20", "50", "70", "70", "75", "60", "100", "70", "70", "75")
                .GridRows("Auto", "Auto", "Auto")
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
                .GridLineY(new GridPoint(9, 0), 3, _frozenLine, GridPlacement.Head)
                .GridLineY(new GridPoint(10, 0), 3)
                .AddBinding(0, 0, this.BindToCheckBox().WithStyle(Styles.CheckBox))
                .AddBinding(1, 0, _.SalesOrderID.BindToColumnHeader("ID"))
                .AddBinding(2, 0, _.SalesOrderNumber.BindToColumnHeader("Number"))
                .AddBinding(3, 0, _.DueDate.BindToColumnHeader("Due Date"))
                .AddBinding(4, 0, _.ShipDate.BindToColumnHeader("Ship Date"))
                .AddBinding(5, 0, _.Status.BindToColumnHeader("Status"))
                .AddBinding(6, 0, _.PurchaseOrderNumber.BindToColumnHeader("PO Number"))
                .AddBinding(7, 0, _.SubTotal.BindToColumnHeader("Sub Total"))
                .AddBinding(8, 0, _.TaxAmt.BindToColumnHeader("Tax Amt"))
                .AddBinding(9, 0, _.TotalDue.BindToColumnHeader("Total Due"))
                .AddBinding(0, 1, _.BindToCheckBox().WithStyle(Styles.CheckBox))
                .AddBinding(1, 1, _.SalesOrderID.BindToHyperlink(ApplicationCommands.Open).WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(2, 1, _.SalesOrderNumber.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(3, 1, _.DueDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock))
                .AddBinding(4, 1, _.ShipDate.BindToTextBlock("{0:d}").WithStyle(Styles.RightAlignedTextBlock))
                .AddBinding(5, 1, _.Status.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(6, 1, _.PurchaseOrderNumber.BindToTextBlock().WithStyle(Styles.LeftAlignedTextBlock))
                .AddBinding(7, 1, _.SubTotal.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock))
                .AddBinding(8, 1, _.TaxAmt.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock))
                .AddBinding(9, 1, _.TotalDue.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock).AddBehavior(new TotalDueConditionalFormat(_.TotalDue)))
                .AddBinding(2, 2, 8, 2, "Total: ".BindToLabel().WithStyle(Styles.Label).AdhereToFrozenRight())
                .AddBinding(9, 2, CalcTotalAmtFunc.BindToTextBlock("{0:C}").WithStyle(Styles.RightAlignedTextBlock).AddBehavior(new TotalAmtConditionalFormat(CalcTotalAmtFunc)))
                .AddBehavior(new RowViewAlternation());
            }

            public void EnsureVisible(int? salesOrderId)
            {
                if (!salesOrderId.HasValue)
                    return;

                var current = GetRow(salesOrderId.Value);
                if (current != null)
                {
                    CurrentRow = current;
                    Scrollable.EnsureCurrentRowVisible();
                }
            }

            private RowPresenter GetRow(int salesOrderId)
            {
                return Match(new SalesOrderHeader.PK(salesOrderId));
            }
        }
    }
}
