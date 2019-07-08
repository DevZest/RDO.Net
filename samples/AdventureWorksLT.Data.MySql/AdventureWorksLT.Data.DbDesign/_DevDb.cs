#if DbDesign
using DevZest.Data.DbDesign;
using MySql.Data.MySqlClient;
using System.Text;

namespace DevZest.Samples.AdventureWorksLT
{
    [EmptyDb]
    public sealed class _DevDb : DbSessionProvider<Db>
    {
        public override Db Create(string projectPath)
        {
            CreateEmptyDb();
            return new Db("Server=127.0.0.1;Port=3306;Database=AdventureWorksLT_Dev;Uid=root;Allow User Variables=True");
        }

        private static void CreateEmptyDb()
        {
            var log = new StringBuilder();
            using (var connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Uid=root"))
            {
                connection.Open();
                var sqlText =
@"set @@sql_notes = 0;
drop database if exists AdventureWorksLT_Dev;
set @@sql_notes = 1;

create database AdventureWorksLT_Dev;
";
                var command = new MySqlCommand(sqlText, connection);
                command.ExecuteNonQuery();
            }
        }

    }
}
#endif
