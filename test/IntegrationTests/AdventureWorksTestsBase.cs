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
            return Db.Open(GetConnectionString());
        }

        protected Db OpenDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return Db.Open(GetConnectionString(), db =>
            {
                db.SetLog(s => log.Append(s), logCategory);
            });
        }

        protected Task<Db> OpenDbAsync()
        {
            return Db.OpenAsync(GetConnectionString());
        }

        protected Task<Db> OpenDbAsync(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return Db.OpenAsync(GetConnectionString(), db =>
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
    }
}
