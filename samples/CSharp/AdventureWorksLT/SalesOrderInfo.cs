using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [InvisibleToDbDesigner]
    public class SalesOrderInfo : SalesOrderHeader
    {
        static SalesOrderInfo()
        {
            RegisterProjection((SalesOrderInfo _) => _.Customer);
            RegisterProjection((SalesOrderInfo _) => _.ShipToAddress);
            RegisterProjection((SalesOrderInfo _) => _.BillToAddress);
            RegisterChildModel((SalesOrderInfo _) => _.SalesOrderDetails);
        }

        public Customer.Lookup Customer { get; private set; }
        public Address.Lookup ShipToAddress { get; private set; }
        public Address.Lookup BillToAddress { get; private set; }

        public SalesOrderInfoDetail SalesOrderDetails { get; private set; }
    }
}
