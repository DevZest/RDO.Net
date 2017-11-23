using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>Defines name of the column in the database.</summary>
    public sealed class DbColumnNameAttribute : ColumnAttribute
    {
        /// <summary>Initializes a new instance of <see cref="DbColumnNameAttribute"/> object.</summary>
        /// <param name="dbColumnName">The database column name.</param>
        public DbColumnNameAttribute(string dbColumnName)
        {
            DbColumnName = dbColumnName;
        }

        /// <summary>Gets or sets the column name for the column.</summary>
        public string DbColumnName { get; private set; }

        /// <inheritdoc/>
        protected internal sealed override void Initialize(Column column)
        {
            column.DbColumnName = DbColumnName;
        }
    }
}
