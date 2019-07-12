using DevZest.Data;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace Movies
{
    public partial class Db : SqlSession
    {
        public Db(string connectionString)
            : this(new SqlConnection(connectionString))
        {
        }

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Movie> _movie;
        public DbTable<Movie> Movie
        {
            get
            {
                return GetTable(ref _movie);
            }
        }
    }
}
