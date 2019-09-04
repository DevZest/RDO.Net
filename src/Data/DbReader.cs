using DevZest.Data.Primitives;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Base class of database reader.
    /// </summary>
    public abstract class DbReader : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbReader"/> for specified model.
        /// </summary>
        /// <param name="model">The specified model.</param>
        protected DbReader(Model model)
        {
            model.VerifyNotNull(nameof(model));
            Model = model;
            IsBof = true;
            IsEof = false;
        }

        /// <summary>
        /// Gets the ADO.Net database data reader.
        /// </summary>
        /// <returns>The ADO.Net database data reader.</returns>
        protected abstract DbDataReader GetDbDataReader();

        /// <summary>
        /// Gets the model associated with this DbReader.
        /// </summary>
        public Model Model { get; private set; }

        private IDbSet DbSet
        {
            get { return Model.DataSource as IDbSet; }
        }

        private DbSession DbSession
        {
            get { return DbSet == null ? null : DbSet.DbSession; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var dbSession = DbSession;
            if (dbSession == null)
                return string.Empty;

            return dbSession.GetSqlString(DbSet.QueryStatement);
        }

        /// <summary>
        /// Closes and disposes this DbReader.
        /// </summary>
        public virtual void Close()
        {
            if (!IsClosed)
                GetDbDataReader().Dispose();
        }

        /// <summary>
        /// Gets a values indicating whether this DbReader is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return GetDbDataReader().IsClosed; }
        }

        /// <summary>
        /// Gets a value that reports whether the reader position is at beginning-of-file.
        /// </summary>
        public bool IsBof { get; private set; }

        /// <summary>
        /// Gets a value that reports whether the reader position is at end-of-file.
        /// </summary>
        public bool IsEof { get; private set; }

        /// <summary>
        /// Read to next record.
        /// </summary>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns><see langword="true" /> if successfully read to next record, otherwise <see langword="false" />.</returns>
        public async Task<bool> ReadAsync(CancellationToken ct = default(CancellationToken))
        {
            var result = await GetDbDataReader().ReadAsync(ct);
            IsBof = false;
            if (!result)
                IsEof = true;
            return result;
        }

        /// <summary>
        /// Gets the value of specified column.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The data value.</returns>
        public virtual object this[int ordinal]
        {
            get
            {
                var reader = GetDbDataReader();
                return reader.IsDBNull(ordinal) ? null : reader[ordinal];
            }
        }

        /// <summary>
        /// Gets the value of specified column as Boolean.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Boolean? GetBoolean(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(bool))
                return reader.GetBoolean(ordinal);

            return (bool)Convert.ChangeType(reader[ordinal], typeof(bool));
        }

        /// <summary>
        /// Gets the value of specified column as Byte.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Byte? GetByte(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(byte))
                return reader.GetByte(ordinal);

            return (byte)Convert.ChangeType(reader[ordinal], typeof(byte));
        }

        /// <summary>
        /// Gets the value of specified column as Char.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Char? GetChar(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(char))
                return reader.GetChar(ordinal);

            return (char)Convert.ChangeType(reader[ordinal], typeof(char));
        }

        /// <summary>
        /// Gets the value of specified column as DateTime.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual DateTime? GetDateTime(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(DateTime))
                return reader.GetDateTime(ordinal);

            return (DateTime)Convert.ChangeType(reader[ordinal], typeof(DateTime));
        }

        /// <summary>
        /// Gets the value of specified column as Decimal.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Decimal? GetDecimal(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Decimal))
                return reader.GetDecimal(ordinal);

            return (Decimal)Convert.ChangeType(reader[ordinal], typeof(Decimal));
        }

        /// <summary>
        /// Gets the value of specified column as Double.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Double? GetDouble(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Double))
                return reader.GetDouble(ordinal);

            return (Double)Convert.ChangeType(reader[ordinal], typeof(Double));
        }

        /// <summary>
        /// Gets the value of specified column as Guid.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Guid? GetGuid(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Guid))
                return reader.GetGuid(ordinal);

            return (Guid)Convert.ChangeType(reader[ordinal], typeof(Guid));
        }

        /// <summary>
        /// Gets the value of specified column as Int16.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Int16? GetInt16(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int16))
                return reader.GetInt16(ordinal);

            return (Int16)Convert.ChangeType(reader[ordinal], typeof(Int16));
        }

        /// <summary>
        /// Gets the value of specified column as Int32.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Int32? GetInt32(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int32))
                return reader.GetInt32(ordinal);

            return (Int32)Convert.ChangeType(reader[ordinal], typeof(Int32));
        }

        /// <summary>
        /// Gets the value of specified column as Int64.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Int64? GetInt64(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Int64))
                return reader.GetInt64(ordinal);

            return (Int64)Convert.ChangeType(reader[ordinal], typeof(Int64));
        }

        /// <summary>
        /// Gets the value of specified column as Single.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
        public virtual Single? GetSingle(int ordinal)
        {
            var reader = GetDbDataReader();

            if (reader.IsDBNull(ordinal))
                return null;

            if (reader.GetFieldType(ordinal) == typeof(Single))
                return reader.GetFloat(ordinal);

            return (Single)Convert.ChangeType(reader[ordinal], typeof(Single));
        }

        /// <summary>
        /// Gets the value of specified column as String.
        /// </summary>
        /// <param name="ordinal">The column ordinal.</param>
        /// <returns>The value.</returns>
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

        /// <summary>
        /// Releases the resources owned by this DbReader.
        /// </summary>
        public void Dispose()
        { 
            Dispose(true);
        }

        /// <summary>
        /// Releases the resources owned by this DbReader.
        /// </summary>
        /// <param name="disposing">If set to <see langword="true" />, the method is invoked directly and will dispose manage
        /// and unmanaged resources; If set to <see langword="false"/> the method is being called by the garbage collector finalizer
        /// and should only release unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.Close();
        }

        #endregion
    }
}
