﻿
using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    internal sealed class SalesOrderMockDb : MockDb<Db>
    {
        private static DataSet<SalesOrderHeader> s_salesOrderHeader = DataSet<SalesOrderHeader>.ParseJson(Strings.Mock_SalesOrderHeader);
        private static DataSet<SalesOrderDetail> s_salesOrderDetail = DataSet<SalesOrderDetail>.ParseJson(Strings.Mock_SalesOrderDetail);

        public SalesOrderMockDb()
            : this(s_salesOrderHeader, s_salesOrderDetail)
        {
        }

        public SalesOrderMockDb(DataSet<SalesOrderHeader> salesOrderHeader, DataSet<SalesOrderDetail> salesOrderDetail)
        {
            _salesOrderHeader = salesOrderHeader;
            _salesOrderDetail = salesOrderDetail;
        }

        private DataSet<SalesOrderHeader> _salesOrderHeader;
        private DataSet<SalesOrderDetail> _salesOrderDetail;

        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail, _salesOrderDetail);
            Mock(Db.SalesOrderHeader, _salesOrderHeader);
        }
    }
}
