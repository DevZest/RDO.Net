using DevZest.Data.DbDesign;
using System.IO;

namespace Movies
{
    [EmptyDb]
    public sealed class _DevDb : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.Combine(projectPath, @"LocalDb");
            string attachDbFilename = Path.Combine(dbFolder, "Movies.mdf");
            File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, true);
            File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "Movies_log.ldf"), true);
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
