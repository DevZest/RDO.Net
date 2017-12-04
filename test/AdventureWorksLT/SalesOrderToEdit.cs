using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderToEdit : SalesOrder
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

        public SalesOrderToEdit()
        {
            SetExtender<Ext>();
        }

        protected override void OnChildModelsMounted()
        {
            SalesOrderDetails.SetExtender<DetailExt>();
            base.OnChildModelsMounted();
        }

        protected override void OnChildDataSetsCreated()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
            base.OnChildDataSetsCreated();
        }
    }
}
