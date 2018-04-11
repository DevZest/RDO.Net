using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;
using System.Windows.Controls;

namespace AdventureWorks.SalesOrders
{
    partial class SalesOrderForm
    {
        private class DetailPresenter : DataPresenter<SalesOrderDetailInfo>, ForeignKeyBox.ILookupService
        {
            public DetailPresenter(Window ownerWindow)
            {
                _ownerWindow = ownerWindow;
            }

            private Window _ownerWindow;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var ext = _.GetExtender<SalesOrderDetailInfo.Ext>();
                builder.GridRows("Auto", "20")
                    .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto")
                    .GridLineX(new GridPoint(0, 2), 7)
                    .GridLineY(new GridPoint(2, 1), 1).GridLineY(new GridPoint(3, 1), 1).GridLineY(new GridPoint(4, 1), 1)
                    .GridLineY(new GridPoint(5, 1), 1).GridLineY(new GridPoint(6, 1), 1).GridLineY(new GridPoint(7, 1), 1)
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AllowDelete()
                    .AddBinding(0, 0, this.BindToGridHeader())
                    .AddBinding(1, 0, ext.Product.ProductNumber.BindToColumnHeader("Product No."))
                    .AddBinding(2, 0, ext.Product.Name.BindToColumnHeader("Product"))
                    .AddBinding(3, 0, _.UnitPrice.BindToColumnHeader("Unit Price"))
                    .AddBinding(4, 0, _.UnitPriceDiscount.BindToColumnHeader("Discount"))
                    .AddBinding(5, 0, _.OrderQty.BindToColumnHeader("Qty"))
                    .AddBinding(6, 0, _.LineTotal.BindToColumnHeader("Total"))
                    .AddBinding(0, 1, _.BindToRowHeader())
                    .AddBinding(1, 1, _.Product.BindToForeignKeyBox(ext.Product, GetProductNumber).MergeIntoGridCell(ext.Product.ProductNumber.BindToTextBlock()))
                    .AddBinding(2, 1, ext.Product.Name.BindToTextBlock().AddToGridCell())
                    .AddBinding(3, 1, _.UnitPrice.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(4, 1, _.UnitPriceDiscount.BindToTextBox(new PercentageConverter()).MergeIntoGridCell(_.UnitPriceDiscount.BindToTextBlock("{0:P}")))
                    .AddBinding(5, 1, _.OrderQty.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(6, 1, _.LineTotal.BindToTextBlock("{0:C}").AddToGridCell());
            }

            private static string GetProductNumber(ColumnValueBag valueBag, Product.Key productKey, Product.Lookup productLookup)
            {
                return valueBag.GetValue(productLookup.ProductNumber);
            }

            bool ForeignKeyBox.ILookupService.CanLookup(PrimaryKey foreignKey)
            {
                if (foreignKey == _.Product)
                    return true;
                else
                    return false;
            }

            void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
            {
                if (foreignKeyBox.ForeignKey == _.Product)
                {
                    var dialogWindow = new ProductLookupWindow();
                    dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(_.ProductID));
                }
                else
                    throw new NotSupportedException();
            }

            protected override bool ConfirmDelete()
            {
                return MessageBox.Show(string.Format("Are you sure you want to delete selected {0} rows?", SelectedRows.Count), "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            }
        }
    }
}
