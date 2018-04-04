using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelExtender(typeof(Ext))]
    public class SalesOrderInfo : SalesOrder
    {
        public class Ext : ModelExtender
        {
            static Ext()
            {
                RegisterChildExtender((Ext _) => _.Customer);
                RegisterChildExtender((Ext _) => _.ShipToAddress);
                RegisterChildExtender((Ext _) => _.BillToAddress);
            }

            public Customer.Lookup Customer { get; private set; }
            public Address.Lookup ShipToAddress { get; private set; }
            public Address.Lookup BillToAddress { get; private set; }
        }

        public class DetailExt : ModelExtender
        {
            static DetailExt()
            {
                RegisterChildExtender((DetailExt _) => _.Product);
            }

            public Product.Lookup Product { get; private set; }
        }

        [ModelExtender(typeof(DetailExt))]
        public override SalesOrderDetail SalesOrderDetails
        {
            get { return base.SalesOrderDetails; }
        }

        [Computation(IsAggregate = true)]
        private void ComputeSubTotal()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
        }
    }
}
