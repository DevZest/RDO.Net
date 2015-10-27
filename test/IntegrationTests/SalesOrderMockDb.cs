
using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    internal sealed class SalesOrderMockDb : MockDb<Db>
    {
        private static DataSet<SalesOrder> s_salesOrder = DataSet<SalesOrder>.ParseJson(Strings.Mock_SalesOrder);
        private static DataSet<SalesOrderDetail> s_salesOrderDetail = DataSet<SalesOrderDetail>.ParseJson(Strings.Mock_SalesOrderDetail);

        public SalesOrderMockDb()
            : this(s_salesOrder, s_salesOrderDetail)
        {
        }

        public SalesOrderMockDb(DataSet<SalesOrder> salesOrder, DataSet<SalesOrderDetail> salesOrderDetail)
        {
            _salesOrder = salesOrder;
            _salesOrderDetail = salesOrderDetail;
        }

        private DataSet<SalesOrder> _salesOrder;
        private DataSet<SalesOrderDetail> _salesOrderDetail;

        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetails, _salesOrderDetail);
            Mock(Db.SalesOrders, _salesOrder);
        }
    }
}
