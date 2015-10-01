
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    /// <summary>Defines name of the column.</summary>
    public sealed class ColumnNameAttribute : ColumnAttribute
    {
        /// <summary>Initializes a new instance of <see cref="ColumnNameAttribute"/> object.</summary>
        /// <param name="columnName">The column name.</param>
        public ColumnNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>Gets or sets the column name for the column.</summary>
        public string ColumnName { get; private set; }

        /// <inheritdoc/>
        protected internal sealed override void Initialize(Column column)
        {
            column.ColumnName = ColumnName;
        }
    }
}
