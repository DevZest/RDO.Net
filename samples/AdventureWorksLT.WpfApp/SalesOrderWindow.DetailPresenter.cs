using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class SalesOrderWindow
    {
        private class DetailPresenter : DataPresenter<SalesOrderInfoDetail>, ForeignKeyBox.ILookupService, DataView.IPasteAppendService
        {
            public DetailPresenter(Window ownerWindow)
            {
                _ownerWindow = ownerWindow;
            }

            private readonly Window _ownerWindow;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var product = _.Product;
                builder.GridRows("Auto", "20")
                    .GridColumns("20", "*", "*", "Auto", "Auto", "Auto", "Auto")
                    .WithFrozenTop(1)
                    .GridLineX(new GridPoint(0, 2), 7)
                    .GridLineY(new GridPoint(2, 1), 1).GridLineY(new GridPoint(3, 1), 1).GridLineY(new GridPoint(4, 1), 1)
                    .GridLineY(new GridPoint(5, 1), 1).GridLineY(new GridPoint(6, 1), 1).GridLineY(new GridPoint(7, 1), 1)
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AllowDelete()
                    .AddBinding(0, 0, this.BindToGridHeader())
                    .AddBinding(1, 0, product.ProductNumber.BindToColumnHeader("Product No."))
                    .AddBinding(2, 0, product.Name.BindToColumnHeader("Product"))
                    .AddBinding(3, 0, _.UnitPrice.BindToColumnHeader("Unit Price"))
                    .AddBinding(4, 0, _.UnitPriceDiscount.BindToColumnHeader("Discount"))
                    .AddBinding(5, 0, _.OrderQty.BindToColumnHeader("Qty"))
                    .AddBinding(6, 0, _.LineTotal.BindToColumnHeader("Total"))
                    .AddBinding(0, 1, _.BindTo<RowHeader>())
                    .AddBinding(1, 1, _.FK_Product.BindToForeignKeyBox(product, GetProductNumber).MergeIntoGridCell(product.ProductNumber.BindToTextBlock()).WithSerializableColumns(_.ProductID, product.ProductNumber))
                    .AddBinding(2, 1, product.Name.BindToTextBlock().AddToGridCell().WithSerializableColumns(product.Name))
                    .AddBinding(3, 1, _.UnitPrice.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(4, 1, _.UnitPriceDiscount.BindToTextBox(new PercentageConverter()).MergeIntoGridCell(_.UnitPriceDiscount.BindToTextBlock("{0:P}")))
                    .AddBinding(5, 1, _.OrderQty.BindToTextBox().MergeIntoGridCell())
                    .AddBinding(6, 1, _.LineTotal.BindToTextBlock("{0:C}").AddToGridCell().WithSerializableColumns(_.LineTotal));
            }

            private static string GetProductNumber(ColumnValueBag valueBag, Product.PK productKey, Product.Lookup productLookup)
            {
                return valueBag.GetValue(productLookup.ProductNumber);
            }

            bool ForeignKeyBox.ILookupService.CanLookup(CandidateKey foreignKey)
            {
                if (foreignKey == _.FK_Product)
                    return true;
                else
                    return false;
            }

            void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
            {
                if (foreignKeyBox.ForeignKey == _.FK_Product)
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

            bool DataView.IPasteAppendService.Verify(IReadOnlyList<ColumnValueBag> data)
            {
                var foreignKeys = DataSet<Product.Ref>.Create();
                for (int i = 0; i < data.Count; i++)
                {
                    var valueBag = data[i];
                    var productId = valueBag.ContainsKey(_.ProductID) ? valueBag[_.ProductID] : null;
                    foreignKeys.AddRow((_, dataRow) =>
                    {
                        _.ProductID.SetValue(dataRow, productId);
                    });
                }

                if (!App.Execute((db, ct) => db.LookupAsync(foreignKeys, ct), Window.GetWindow(View), out var lookup))
                    return false;

                Debug.Assert(lookup.Count == data.Count);
                var product = _.Product;
                for (int i = 0; i < lookup.Count; i++)
                {
                    data[i].SetValue(product.Name, lookup._.Name[i]);
                    data[i].SetValue(product.ProductNumber, lookup._.ProductNumber[i]);
                }
                return true;
            }
        }
    }
}
