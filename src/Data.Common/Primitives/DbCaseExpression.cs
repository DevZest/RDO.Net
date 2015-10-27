using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    public sealed class DbCaseExpression : DbExpression
    {
        internal DbCaseExpression(DbExpression onExpr,
            IEnumerable<DbExpression> whens,
            IEnumerable<DbExpression> thens,
            DbExpression elseExpr)
        {
            On = onExpr;
            When = new ReadOnlyCollection<DbExpression>(whens.ToList());
            Then = new ReadOnlyCollection<DbExpression>(thens.ToList());
            Else = elseExpr;
        }

        public DbExpression On { get; private set; }

        public ReadOnlyCollection<DbExpression> When { get; private set; }

        public ReadOnlyCollection<DbExpression> Then { get; private set; }

        public DbExpression Else { get; private set; }

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
