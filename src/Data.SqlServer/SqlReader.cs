﻿using DevZest.Data.Primitives;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
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

        protected override DbDataReader GetDbDataReader()
        {
            return SqlDataReader;
        }

        public SqlDataReader SqlDataReader { get; private set; }

        public DateTimeOffset? GetDateTimeOffset(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : new DateTimeOffset?(reader.GetDateTimeOffset(ordinal));
        }

        public TimeSpan? GetTimeSpan(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : new TimeSpan?(reader.GetTimeSpan(ordinal));
        }

        public SqlXml GetSqlXml(int ordinal)
        {
            var reader = SqlDataReader;
            return reader.IsDBNull(ordinal) ? null : reader.GetSqlXml(ordinal);
        }

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
