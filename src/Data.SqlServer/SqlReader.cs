using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Reads data from SQL Server database.
    /// </summary>
    public sealed class SqlReader : DbReader
    {
        internal static async Task<SqlReader> ExecuteAsync(SqlCommand sqlCommand, Model model, CancellationToken cancellationToken)
        {
            var sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
            return new SqlReader(sqlDataReader, model);
        }

        private SqlReader(SqlDataReader sqlDataReader, Model model)
            : base(model)
        {
            Debug.Assert(sqlDataReader != null);
            SqlDataReader = sqlDataReader;
        }

        /// <inheritdoc/>
        protected override DbDataReader GetDbDataReader()
        {
            return SqlDataReader;
        }

        /// <summary>
        /// Gets the SQL Server data reader.
        /// </summary>
        public SqlDataReader SqlDataReader { get; private set; }

        /// <summary>
        /// Retrieves the value of the specified column as nullable <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="ordinal">The zero based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public DateTimeOffset? GetDateTimeOffset(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : new DateTimeOffset?(reader.GetDateTimeOffset(ordinal));
        }

        /// <summary>
        /// Retrieves the value of the specified column as nullable <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="ordinal">The zero based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public TimeSpan? GetTimeSpan(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : new TimeSpan?(reader.GetTimeSpan(ordinal));
        }

        /// <summary>
        /// Retrieves the value of the specified column as <see cref="SqlXml"/>.
        /// </summary>
        /// <param name="ordinal">The zero based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public SqlXml GetSqlXml(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : reader.GetSqlXml(ordinal);
        }

        /// <summary>
        /// Retrieves the value of the specified column as nullable <see cref="char"/>.
        /// </summary>
        /// <param name="ordinal">The zero based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override char? GetChar(int ordinal)
        {
            // SqlDataReader does not support GetChar, must override to call GetChars instead.
            var reader = SqlDataReader;
            if (reader.IsDBNull(ordinal))
                return null;

            var result = new char[1];
            reader.GetChars(ordinal, 0, result, 0, 1);
            return result[0];
        }
    }
}
