using System;

namespace DevZest.Data.Primitives
{
    public sealed class DbConstantExpression : DbExpression
    {
        internal static DbConstantExpression Null = new DbConstantExpression(null, null);

        internal DbConstantExpression(Column column, object value)
        {
            Column = column;
            Value = value;
        }

        public Column Column { get; private set; }

        public object Value { get; private set; }

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
