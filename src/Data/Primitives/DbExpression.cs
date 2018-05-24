namespace DevZest.Data.Primitives
{
    public abstract class DbExpression
    {
        internal DbExpression()
        {
        }

        public abstract void Accept(DbExpressionVisitor visitor);

        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);
    }
}
