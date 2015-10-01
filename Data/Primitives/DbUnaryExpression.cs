using System;

namespace DevZest.Data.Primitives
{
    public sealed class DbUnaryExpression : DbExpression
    {
        public DbUnaryExpression(DbUnaryExpressionKind kind, DbExpression operand)
        {
            Kind = kind;
            Operand = operand;
        }

        public DbUnaryExpressionKind Kind { get; private set; }

        public DbExpression Operand { get; private set; }

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
