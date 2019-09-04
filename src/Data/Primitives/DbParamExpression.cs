using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents parameter in database expression.
    /// </summary>
    public sealed class DbParamExpression : DbExpression
    {
        internal DbParamExpression(Column column, Column sourceColumn, object value)
            : base(column.DataType)
        {
            Debug.Assert(column != null);
            Debug.Assert(sourceColumn == null || sourceColumn.GetType() == column.GetType());
            Column = column;
            SourceColumn = sourceColumn;
            Value = value;
        }

        /// <summary>
        /// Gets the column which contains the parameter value.
        /// </summary>
        public Column Column { get; private set; }

        /// <summary>
        /// Gets the source column which is model member and used for database data type infering.
        /// </summary>
        public Column SourceColumn { get; private set; }

        /// <summary>
        /// Gets the value of the parameter
        /// </summary>
        public object Value { get; private set; }

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
    }
}
