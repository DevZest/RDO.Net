using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ExtraColumns(typeof(Ext))]
    public class SalesOrderInfo : SalesOrder
    {
        public class Ext : LookupContainer
        {
            static Ext()
            {
                Register((Ext _) => _.Customer);
                Register((Ext _) => _.ShipToAddress);
                Register((Ext _) => _.BillToAddress);
            }

            public Customer.Lookup Customer { get; private set; }
            public Address.Lookup ShipToAddress { get; private set; }
            public Address.Lookup BillToAddress { get; private set; }
        }

        public new SalesOrderInfoDetail SalesOrderDetails
        {
            get { return (SalesOrderInfoDetail)base.SalesOrderDetails; }
        }

        protected sealed override SalesOrderDetail CreateSalesOrderDetail()
        {
            return CreateSalesOrderDetailInfo();
        }

        protected virtual SalesOrderInfoDetail CreateSalesOrderDetailInfo()
        {
            return new SalesOrderInfoDetail();
        }
    }
}
