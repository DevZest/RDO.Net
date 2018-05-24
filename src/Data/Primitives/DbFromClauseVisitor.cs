using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbFromClauseVisitor
    {
        public abstract void Visit(DbTableClause table);

        public abstract void Visit(DbSelectStatement select);

        public abstract void Visit(DbJoinClause join);

        public abstract void Visit(DbUnionStatement union);
    }
}
