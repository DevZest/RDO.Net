using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    public sealed class EmptySalesOrderMockDb : DbMock<Db>
    {
        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail);
            Mock(Db.SalesOrderHeader);
        }
    }
}
