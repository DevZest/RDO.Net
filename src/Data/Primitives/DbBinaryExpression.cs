namespace DevZest.Data.Primitives
{
    public sealed class DbBinaryExpression : DbExpression
    {
        public DbBinaryExpression(BinaryExpressionKind kind, DbExpression left, DbExpression right)
        {
            left.VerifyNotNull(nameof(left));
            right.VerifyNotNull(nameof(right));

            Kind = kind;
            Left = left;
            Right = right;
        }

        public BinaryExpressionKind Kind { get; private set; }

        public DbExpression Left { get; private set; }

        public DbExpression Right { get; private set; }

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
