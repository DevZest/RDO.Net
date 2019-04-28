using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class SqlSessionTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void SqlSession_ExecuteTransaction()
        {
            using (var db = CreateDb())
            {
                db.OpenConnectionAsync(default(CancellationToken)).Wait();
                db.ExecuteTransaction(() =>
                {
                    db.GetSalesOrderInfoAsync(1).Wait();
                });
            }
        }
    }
}
