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

        public bool Read()
        {
            var result = GetDbDataReader().Read();
            IsBof = false;
            if (!result)
                IsEof = true;
            return result;
        }

        public async Task<bool> ReadAsync()
        {
            var result = await ReadAsync(CancellationToken.None);
            IsBof = false;
            if (!result)
                IsEof = true;
            return result;
        }

        public Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return GetDbDataReader().ReadAsync(cancellationToken);
        }

        public object this[int ordinal]
        {
            get
            {
                var reader = GetDbDataReader();
                return reader.IsDBNull(ordinal) ? null : reader[ordinal];
            }
        }

        public Boolean? GetBoolean(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Boolean?(reader.GetBoolean(ordinal));
        }

        public Byte? GetByte(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Byte?(reader.GetByte(ordinal));
        }

        public Char? GetChar(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Char?(reader.GetChar(ordinal));
        }

        public DateTime? GetDateTime(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new DateTime?(reader.GetDateTime(ordinal));
        }

        public Decimal? GetDecimal(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Decimal?(reader.GetDecimal(ordinal));
        }

        public Double? GetDouble(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Double?(reader.GetDouble(ordinal));
        }

        public Guid? GetGuid(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Guid?(reader.GetGuid(ordinal));
        }

        public Int16? GetInt16(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Int16?(reader.GetInt16(ordinal));
        }

        public Int32? GetInt32(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Int32?(reader.GetInt32(ordinal));
        }

        public Int64? GetInt64(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Int64?(reader.GetInt64(ordinal));
        }

        public Single? GetSingle(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : new Single?(reader.GetFloat(ordinal));
        }

        public String GetString(int ordinal)
        {
            var reader = GetDbDataReader();
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
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
