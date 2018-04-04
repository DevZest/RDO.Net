using DevZest.Samples.AdventureWorksLT;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class AdventureWorksTestsBase
    {
        protected Db OpenDb()
        {
            return new Db(GetConnectionString()).Open();
        }

        protected Db OpenDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            }).Open();
        }

        protected Task<Db> OpenDbAsync()
        {
            return new Db(GetConnectionString()).OpenAsync();
        }

        protected Task<Db> OpenDbAsync(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            }).OpenAsync();
        }

        private static string GetConnectionString()
        {
            string mdfFilename = "AdventureWorksLT.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        protected DataSet<SalesOrderInfo> GetSalesOrderInfo(int salesOrderID)
        {
            using (var db = OpenDb())
            {
                return db.GetSalesOrderInfo(salesOrderID).ToDataSet();
            }
        }
    }
}
