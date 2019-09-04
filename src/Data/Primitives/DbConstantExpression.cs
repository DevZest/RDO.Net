using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a database constant expression.
    /// </summary>
    public sealed class DbConstantExpression : DbExpression
    {
        internal static DbConstantExpression Null = new DbConstantExpression(typeof(object), null, null);

        internal DbConstantExpression(Column column, object value)
            : this(column.DataType, column, value)
        {
        }

        private DbConstantExpression(Type dataType, Column column, object value)
            : base(dataType)
        {
            Column = column;
            Value = value;
        }

        /// <summary>
        /// Gets the expression column.
        /// </summary>
        public Column Column { get; private set; }

        /// <summary>
        /// Gets the constant value.
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
