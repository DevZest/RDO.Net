using DevZest.Data.DbInit;
using System.IO;
using System.Reflection;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class DbProvider : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
