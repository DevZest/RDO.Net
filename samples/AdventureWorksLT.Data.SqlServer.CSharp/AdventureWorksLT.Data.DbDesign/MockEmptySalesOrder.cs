using DevZest.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <remarks><see cref="SalesOrder"/> and <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) and
    /// parent-child relationship.</remarks>
    public sealed class MockEmptySalesOrder : DbMock<Db>
    {
        public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            return new MockEmptySalesOrder().MockAsync(db, progress, ct);
        }

        protected override void Initialize()
        {
            // The order of mocking table does not matter, the dependencies will be sorted out automatically.
            Mock(Db.SalesOrderDetail);
            Mock(Db.SalesOrderHeader);
        }
    }
}
