#if DbDesign
using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class DesignTimeDb : DesignTimeDb<Db>
    {
        public override Db Create(string projectPath)
        {
            var connectionString = "Server=127.0.0.1;Port=3306;Database=AdventureWorksLT_Design;Uid=root;Allow User Variables=True";
            return new Db(connectionString);
        }
    }
}
#endif