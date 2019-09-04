using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a column in database expression.
    /// </summary>
    public sealed class DbColumnExpression : DbExpression
    {
        internal DbColumnExpression(Column column)
            : base(column.DataType)
        {
            Debug.Assert(column != null);
            Column = column;
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        public Column Column { get; private set; }

        /// <summary>
        /// Gets the database column name.
        /// </summary>
        public string DbColumnName
        {
            get { return Column.DbColumnName; }
        }

        /// <inheritdoc />
        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return DbColumnName;
        }
    }
}
