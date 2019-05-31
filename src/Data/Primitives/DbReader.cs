using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class DbReader : IDisposable
    {
        protected DbReader(Model model)
        {
            model.VerifyNotNull(nameof(model));
            Model = model;
            IsBof = true;
            IsEof = false;
        }

        protected abstract DbDataReader GetDbDataReader();

        public Model Model { get; private set; }

        private IDbSet DbSet
        {
            get { return Model.DataSource as IDbSet; }
        }

        private DbSession DbSession
        {
            get { return DbSet == null ? null : DbSet.DbSession; }
        }

        public override string ToString()
        {
            var dbSession = DbSession;
            if (dbSession == null)
                return string.Empty;

            return dbSession.GetSqlString(DbSet.QueryStatement);
        }

        public virtual void Close()
        {
            if (!IsClosed)
                GetDbDataReader().Dispose();
        }

        public bool IsClosed
        {
            get { return GetDbDataReader().IsClosed; }
        }

        public bool IsBof { get; private set; }

        public bool IsEof { get; private set; }

        public async Task<bool> ReadAsync(CancellationToken ct = default(CancellationToken))
        {
            var result = await GetDbDataReader().ReadAsync(ct);
            IsBof = false;
            if (!result)
                IsEof = true;
            return result;
        }

        public virtual object this[int ordinal]
        {
            get
            {
                var reader = GetDbDataReader();
                return reader.IsDBNull(ordinal) ? null : reader[ordinal];
            }
        }

        public virtual Boolean? GetBoolean(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(bool))
                return reader.GetBoolean(ordinal);

            return (bool)Convert.ChangeType(reader[ordinal], typeof(bool));
        }

        public virtual Byte? GetByte(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(byte))
                return reader.GetByte(ordinal);

            return (byte)Convert.ChangeType(reader[ordinal], typeof(byte));
        }

        public virtual Char? GetChar(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(char))
                return reader.GetChar(ordinal);

            return (char)Convert.ChangeType(reader[ordinal], typeof(char));
        }

        public virtual DateTime? GetDateTime(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(DateTime))
                return reader.GetDateTime(ordinal);

            return (DateTime)Convert.ChangeType(reader[ordinal], typeof(DateTime));
        }

        public virtual Decimal? GetDecimal(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Decimal))
                return reader.GetDecimal(ordinal);

            return (Decimal)Convert.ChangeType(reader[ordinal], typeof(Decimal));
        }

        public virtual Double? GetDouble(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Double))
                return reader.GetDouble(ordinal);

            return (Double)Convert.ChangeType(reader[ordinal], typeof(Double));
        }

        public virtual Guid? GetGuid(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Guid))
                return reader.GetGuid(ordinal);

            return (Guid)Convert.ChangeType(reader[ordinal], typeof(Guid));
        }

        public virtual Int16? GetInt16(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int16))
                return reader.GetInt16(ordinal);

            return (Int16)Convert.ChangeType(reader[ordinal], typeof(Int16));
        }

        public virtual Int32? GetInt32(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int32))
                return reader.GetInt32(ordinal);

            return (Int32)Convert.ChangeType(reader[ordinal], typeof(Int32));
        }

        public virtual Int64? GetInt64(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int64))
                return reader.GetInt64(ordinal);

            return (Int64)Convert.ChangeType(reader[ordinal], typeof(Int64));
        }

        public virtual Single? GetSingle(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Single))
                return reader.GetFloat(ordinal);

            return (Single)Convert.ChangeType(reader[ordinal], typeof(Single));
        }

        public virtual String GetString(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(string))
                return reader.GetString(ordinal);

            return (String)Convert.ChangeType(reader[ordinal], typeof(String));
        }

        #region IDisposable

        public void Dispose()
        { 
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.Close();
        }

        #endregion
    }
}
