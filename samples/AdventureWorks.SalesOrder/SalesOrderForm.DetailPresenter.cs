using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;

namespace AdventureWorks.SalesOrders
{
    partial class SalesOrderForm
    {
        private class DetailPresenter : DataPresenter<SalesOrderDetail>, ForeignKeyBox.ILookupService
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var ext = _.GetExtension<SalesOrderToEdit.DetailExt>();
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto")
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AddBinding(1, 0, ext.Product.ProductNumber.AsColumnHeader("Product No."))
                    .AddBinding(2, 0, ext.Product.Name.AsColumnHeader("Product"))
                    .AddBinding(3, 0, _.UnitPrice.AsColumnHeader("Unit Price"))
                    .AddBinding(4, 0, _.UnitPriceDiscount.AsColumnHeader("Discount"))
                    .AddBinding(5, 0, _.OrderQty.AsColumnHeader("Qty"))
                    .AddBinding(6, 0, _.LineTotal.AsColumnHeader("Total"))
                    .AddBinding(0, 1, _.AsRowHeader())
                    .AddBinding(1, 1, _.Product.AsForeignKeyBox(ext.Product, GetProductNumber))
                    .AddBinding(2, 1, ext.Product.Name.AsTextBlock())
                    .AddBinding(3, 1, _.UnitPrice.AsTextBox())
                    .AddBinding(4, 1, _.UnitPriceDiscount.AsTextBox())
                    .AddBinding(5, 1, _.OrderQty.AsTextBox())
                    .AddBinding(6, 1, _.LineTotal.AsTextBlock("{0:C}"));
            }

            private static string GetProductNumber(ColumnValueBag valueBag, Product.Key productKey, Product.Lookup productLookup)
            {
                return valueBag.GetValue(productLookup.ProductNumber);
            }

            bool ForeignKeyBox.ILookupService.CanLookup(KeyBase foreignKey)
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
        }
    }
}
