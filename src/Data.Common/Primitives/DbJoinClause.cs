using DevZest.Data.Utilities;
using System;
using System.Collections.ObjectModel;

namespace DevZest.Data.Primitives
{
    public sealed class DbJoinClause : DbFromClause
    {
        public DbJoinClause(DbJoinKind kind, DbFromClause left, DbFromClause right, ReadOnlyCollection<ColumnMapping> on)
        {
            Kind = kind;
            Left = left;
            Right = right;
            On = on;
        }

        public DbJoinKind Kind { get; private set; }

        public DbFromClause Left { get; private set; }

        public DbFromClause Right { get; private set; }

        public ReadOnlyCollection<ColumnMapping> On { get; private set; }

        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
