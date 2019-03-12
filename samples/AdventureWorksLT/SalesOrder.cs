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

        protected sealed override SalesOrderDetail GetSalesOrderDetails()
        {
            return SalesOrderDetails;
        }
    }
}
