using DevZest.Data.Primitives;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    public sealed class MySqlReader : DbReader
    {
        internal static async Task<MySqlReader> ExecuteAsync(MySqlCommand mySqlCommand, Model model, CancellationToken cancellationToken)
        {
            var mySqlDataReader = (MySqlDataReader)(await mySqlCommand.ExecuteReaderAsync(cancellationToken));
            return new MySqlReader(mySqlDataReader, model);
        }

        private MySqlReader(MySqlDataReader mySqlDataReader, Model model)
            : base(model)
        {
            Debug.Assert(mySqlDataReader != null);
            MySqlDataReader = mySqlDataReader;
        }

        protected override DbDataReader GetDbDataReader()
        {
            return MySqlDataReader;
        }

        public MySqlDataReader MySqlDataReader { get; private set; }
    }
}
