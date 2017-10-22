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
            return new Db(GetConnectionString()).OpenConnection();
        }

        protected Db OpenDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            }).OpenConnection();
        }

        protected Task<Db> OpenDbAsync()
        {
            return new Db(GetConnectionString()).OpenConnectionAsync();
        }

        protected Task<Db> OpenDbAsync(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            }).OpenConnectionAsync();
        }

        private static string GetConnectionString()
        {
            string mdfFilename = "AdventureWorksLT.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        protected DataSet<SalesOrderToEdit> GetSalesOrderToEdit(int salesOrderID)
        {
            using (var db = OpenDb())
            {
                return db.GetSalesOrderToEdit(salesOrderID).ToDataSet();
            }
        }
    }
}
