using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                db.OpenConnectionAsync().Wait();
                db.ExecuteTransaction(() =>
                {
                    db.GetSalesOrderInfoAsync(1).Wait();
                });
            }
        }
    }
}
