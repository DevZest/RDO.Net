using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public sealed class DbJoinClause : DbFromClause
    {
        public DbJoinClause(DbJoinKind kind, DbFromClause left, DbFromClause right, IReadOnlyList<ColumnMapping> on)
        {
            Kind = kind;
            Left = left;
            Right = right;
            On = on;
        }

        public DbJoinKind Kind { get; private set; }

        public DbFromClause Left { get; private set; }

        public DbFromClause Right { get; private set; }

        public IReadOnlyList<ColumnMapping> On { get; private set; }

        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override void OnClone(Model model)
        {
        }
    }
}
