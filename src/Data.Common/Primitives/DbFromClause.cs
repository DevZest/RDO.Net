using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbFromClause
    {
        public abstract void Accept(DbFromClauseVisitor visitor);

        public abstract T Accept<T>(DbFromClauseVisitor<T> visitor);

        internal virtual DbExpression GetSubQueryEliminatedSource(DbExpression source)
        {
            return source;
        }

        internal virtual DbFromClause SubQueryEliminatedFromClause
        {
            get { return this; }
        }
    }
}
