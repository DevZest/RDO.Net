#if DbDesign
using DevZest.Data;
using DevZest.Data.Annotations;
using MySql.Data.MySqlClient;
using System.Text;

namespace DevZest.Samples.AdventureWorksLT
{
    [DesignTimeDb(true)]
    public sealed class CleanDesignTimeDb : DesignTimeDb<Db>
    {
        public override Db Create(string projectPath)
        {
            CreateEmptyDb();
            return new Db("Server=127.0.0.1;Port=3306;Database=EmptyDb;Uid=root;Allow User Variables=True");
        }

        private static void CreateEmptyDb()
        {
            var log = new StringBuilder();
            using (var connection = new MySqlConnection("Server=127.0.0.1;Port=3306;Uid=root"))
            {
                connection.Open();
                var sqlText =
@"set @@sql_notes = 0;
drop database if exists EmptyDb;
set @@sql_notes = 1;

create database EmptyDb;
";
                var command = new MySqlCommand(sqlText, connection);
                command.ExecuteNonQuery();
            }
        }

    }
}
#endif
