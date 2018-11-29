using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    internal sealed class EmptySalesOrderMockDb : MockDb<Db>
    {
        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail);
            Mock(Db.SalesOrderHeader);
        }
    }
}
