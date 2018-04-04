using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : SalesOrderHeader
    {
        static SalesOrder()
        {
            RegisterChildModel((SalesOrder x) => x.SalesOrderDetails, (SalesOrderDetail x) => x.SalesOrderHeader);
        }

        public virtual SalesOrderDetail SalesOrderDetails { get; private set; }

        [Computation(IsAggregate = true)]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
        }
    }
}
