using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbFromClauseVisitor<T>
    {
        public abstract T Visit(DbTableClause table);

        public abstract T Visit(DbSelectStatement select);

        public abstract T Visit(DbJoinClause join);

        public abstract T Visit(DbUnionStatement union);
    }
}
