using System;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents database CAST expression.
    /// </summary>
    public sealed class DbCastExpression : DbExpression
    {
        internal DbCastExpression(DbExpression operand, Column sourceColumn, Column targetColumn)
            : base(targetColumn.DataType)
        {
            Operand = operand;
            SourceColumn = sourceColumn;
            TargetColumn = targetColumn;
        }

        /// <summary>
        /// Gets the operand.
        /// </summary>
        public DbExpression Operand { get; private set; }

        /// <summary>
        /// Gets the source data type.
        /// </summary>
        public Type SourceDataType
        {
            get { return SourceColumn.DataType; }
        }

        /// <summary>
        /// Gets the source column.
        /// </summary>
        public Column SourceColumn { get; private set; }

        /// <summary>
        /// Gets the target column.
        /// </summary>
        public Column TargetColumn { get; private set; }

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
