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
            return new Db(App.GetConnectionString());
        }

        protected Db CreateDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(App.GetConnectionString(), db =>
            {
                db.SetLogger(s => log.Append(s), logCategory);
            });
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
