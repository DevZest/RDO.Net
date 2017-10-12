using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using System.Windows.Controls;

namespace AdventureWorks.SalesOrders
{
    public class SalesOrderForm : DataPresenter<SalesOrder>
    {
        public SalesOrderForm()
        {
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
