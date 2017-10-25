using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderToEdit : SalesOrder
    {
        public class Ext : ModelExtension
        {
            static Ext()
            {
                RegisterChildExtension((Ext _) => _.Customer);
                RegisterChildExtension((Ext _) => _.ShipToAddress);
                RegisterChildExtension((Ext _) => _.BillToAddress);
            }

            public Customer.Lookup Customer { get; private set; }
            public Address.Lookup ShipToAddress { get; private set; }
            public Address.Lookup BillToAddress { get; private set; }
        }

        public class DetailExt : ModelExtension
        {
            static DetailExt()
            {
                RegisterChildExtension((DetailExt _) => _.Product);
            }

            public Product.Lookup Product { get; private set; }
        }

        public SalesOrderToEdit()
        {
            SetExtension<Ext>();
        }

        protected override void OnChildModelsMounted()
        {
            SalesOrderDetails.SetExtension<DetailExt>();
            base.OnChildModelsMounted();
        }

        protected override void OnChildDataSetsCreated()
        {
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), false);
            base.OnChildDataSetsCreated();
        }
    }
}
