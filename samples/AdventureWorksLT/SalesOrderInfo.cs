using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderInfo : SalesOrder
    {
        static SalesOrderInfo()
        {
            RegisterColumnGroup((SalesOrderInfo _) => _.LK_Customer);
            RegisterColumnGroup((SalesOrderInfo _) => _.LK_ShipToAddress);
            RegisterColumnGroup((SalesOrderInfo _) => _.LK_BillToAddress);
        }

        public Customer.Lookup LK_Customer { get; private set; }
        public Address.Lookup LK_ShipToAddress { get; private set; }
        public Address.Lookup LK_BillToAddress { get; private set; }

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
