using DevZest.Samples.AdventureWorksLT;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
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
            string mdfFilename = "AdventureWorksLT.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        protected async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID)
        {
            using (var db = CreateDb())
            {
                return await db.GetSalesOrderInfoAsync(salesOrderID);
            }
        }
    }
}
