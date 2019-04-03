#if DbDesign
using DevZest.Data;
using DevZest.Data.Annotations;
using System.IO;

namespace DevZest.Samples.AdventureWorksLT
{
    [DesignTimeDb(true)]
    public sealed class CleanDesignTimeDb : DesignTimeDb<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.Combine(projectPath, @"LocalDb");
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
            File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, true);
            File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.Design_log.ldf"), true);
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
#endif
