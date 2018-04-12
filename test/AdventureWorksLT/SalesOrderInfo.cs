using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelExtender(typeof(ForeignKeyLookup.Ext))]
    public class SalesOrderInfo : SalesOrder
    {
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
