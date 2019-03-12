using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [InvisibleToDbDesigner]
    public class SalesOrder : SalesOrderBase
    {
        static SalesOrder()
        {
            RegisterChildModel((SalesOrder _) => _.SalesOrderDetails, (SalesOrderDetail _) => _.FK_SalesOrderHeader);
        }

        public SalesOrderDetail SalesOrderDetails { get; private set; }

        protected override SalesOrderDetail GetSalesOrderDetails()
        {
            return SalesOrderDetails;
        }
    }
}
