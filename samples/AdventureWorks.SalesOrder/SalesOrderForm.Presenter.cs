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
        private class Presenter : DataPresenter<SalesOrderToEdit>, ForeignKeyBox.ILookupService
        {
            public Presenter(ForeignKeyBox.ILookupService lookupService)
            {
                _lookupService = lookupService;
            }

            private ForeignKeyBox.ILookupService _lookupService;

            bool ForeignKeyBox.ILookupService.CanLookup(KeyBase foreignKey)
            {
                return _lookupService.CanLookup(foreignKey);
            }

            void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
            {
                _lookupService.BeginLookup(foreignKeyBox);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto", "Auto")
                    .GridColumns("580")
                    .AddBinding(0, 0, _.AsSalesOrderHeaderBox())
                    .AddBinding(0, 2, _.AsSalesOrderFooterBox());
            }
        }
    }
}
