using System;

namespace DevZest.Data.Primitives
{
    public sealed class DbCastExpression : DbExpression
    {
        internal DbCastExpression(DbExpression operand, Column sourceColumn, Column targetColumn)
        {
            Operand = operand;
            SourceColumn = sourceColumn;
            TargetColumn = targetColumn;
        }

        public DbExpression Operand { get; private set; }

        public Type SourceDataType
        {
            get { return SourceColumn.DataType; }
        }

        public Column SourceColumn { get; private set; }

        public Column TargetColumn { get; private set; }

        public Type TargetDataType
        {
            get { return TargetColumn.DataType; }
        }

        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
