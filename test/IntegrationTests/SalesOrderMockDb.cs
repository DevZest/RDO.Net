
using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    internal sealed class SalesOrderMockDb : MockDb<Db>
    {
        private static DataSet<SalesOrderHeader> Headers()
        {
            return DataSet<SalesOrderHeader>.ParseJson(Strings.Mock_SalesOrderHeader);
        }

        private static DataSet<SalesOrderDetail> Details()
        {
            return DataSet<SalesOrderDetail>.ParseJson(Strings.Mock_SalesOrderDetail);
        }

        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail, Details);
            Mock(Db.SalesOrderHeader, Headers);
        }
    }
}
