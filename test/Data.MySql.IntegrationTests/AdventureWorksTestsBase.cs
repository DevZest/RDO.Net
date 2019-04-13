using DevZest.Samples.AdventureWorksLT;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    public abstract class AdventureWorksTestsBase
    {
        protected Db CreateDb()
        {
            return new Db(GetConnectionString());
        }

        protected Db CreateDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            });
        }

        private static string GetConnectionString()
        {
            return "Server=127.0.0.1;Port=3306;Database=AdventureWorksLT;Uid=root;Allow User Variables=True";
        }

        protected async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID, CancellationToken ct = default(CancellationToken))
        {
            using (var db = CreateDb())
            {
                return await db.GetSalesOrderInfoAsync(salesOrderID, ct);
            }
        }
    }
}
