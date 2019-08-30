#if DbInit
using DevZest.Data.DbInit;
using System.IO;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class DevDb : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.Combine(projectPath, @"LocalDb");
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.DbInit.mdf");
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
#endif