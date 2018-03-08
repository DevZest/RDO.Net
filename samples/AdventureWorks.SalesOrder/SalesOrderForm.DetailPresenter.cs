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
        private class DetailPresenter : DataPresenter<SalesOrderDetail>, ForeignKeyBox.ILookupService, RowHeader.IDeletingConfirmation
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var ext = _.GetExtender<SalesOrderToEdit.DetailExt>();
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto")
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AddBinding(0, 0, this.BindToGridHeader())
                    .AddBinding(1, 0, ext.Product.ProductNumber.BindToColumnHeader("Product No."))
                    .AddBinding(2, 0, ext.Product.Name.BindToColumnHeader("Product"))
                    .AddBinding(3, 0, _.UnitPrice.BindToColumnHeader("Unit Price"))
                    .AddBinding(4, 0, _.UnitPriceDiscount.BindToColumnHeader("Discount"))
                    .AddBinding(5, 0, _.OrderQty.BindToColumnHeader("Qty"))
                    .AddBinding(6, 0, _.LineTotal.BindToColumnHeader("Total"))
                    .AddBinding(0, 1, _.BindToRowHeader())
                    .AddBinding(1, 1, _.Product.BindToForeignKeyBox(ext.Product, GetProductNumber).AddToGridCell())
                    .AddBinding(2, 1, ext.Product.Name.BindToTextBlock().AddToGridCell())
                    .AddBinding(3, 1, _.UnitPrice.BindToTextBox().AddToGridCell())
                    .AddBinding(4, 1, _.UnitPriceDiscount.BindToTextBox().AddToGridCell())
                    .AddBinding(5, 1, _.OrderQty.BindToTextBox().AddToGridCell())
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
                    MessageBox.Show("Lookup!");
                }
                else
                    throw new NotSupportedException();
            }

            bool RowHeader.IDeletingConfirmation.Confirm()
            {
                return MessageBox.Show(string.Format("Are you sure you want to delete selected {0} rows?", SelectedRows.Count), "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            }
        }
    }
}
