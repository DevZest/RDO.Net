#if DbInit
using DevZest.Data.DbInit;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class DevDb : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            var connectionString = "Server=127.0.0.1;Port=3306;Database=AdventureWorksLT_Dev;Uid=root;Allow User Variables=True";
            return new Db(connectionString);
        }
    }
}
#endif