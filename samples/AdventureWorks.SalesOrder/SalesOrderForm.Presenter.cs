using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;

namespace AdventureWorks.SalesOrders
{
    partial class SalesOrderForm
    {
        private class Presenter : DataPresenter<SalesOrder.Edit>, ForeignKeyBox.ILookupService
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto", "Auto")
                    .GridColumns("580")
                    .AddBinding(0, 0, _.AsSalesOrderHeaderBox())
                    .AddBinding(0, 2, _.AsSalesOrderFooterBox());
            }

            public bool CanLookup(KeyBase foreignKey)
            {
                return true;
            }

            public ColumnValueBag Lookup(KeyBase foreignKey)
            {
                MessageBox.Show("Lookup!");
                return null;
            }
        }
    }
}
