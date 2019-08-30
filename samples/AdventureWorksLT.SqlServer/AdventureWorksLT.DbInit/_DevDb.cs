#if DbInit
using DevZest.Data.DbInit;
using System.IO;

namespace DevZest.Samples.AdventureWorksLT
{
    [EmptyDb]
    public sealed class _DevDb : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.Combine(projectPath, @"LocalDb");
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.DbInit.mdf");
            File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, true);
            File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.DbInit_log.ldf"), true);
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
#endif
