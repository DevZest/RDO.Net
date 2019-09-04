using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a databse CASE expression.
    /// </summary>
    public sealed class DbCaseExpression : DbExpression
    {
        internal DbCaseExpression(Type dataType, DbExpression onExpr,
            IEnumerable<DbExpression> whens,
            IEnumerable<DbExpression> thens,
            DbExpression elseExpr)
            : base(dataType)
        {
            On = onExpr;
            When = new ReadOnlyCollection<DbExpression>(whens.ToList());
            Then = new ReadOnlyCollection<DbExpression>(thens.ToList());
            Else = elseExpr;
        }

        /// <summary>
        /// Gets the ON expression.
        /// </summary>
        public DbExpression On { get; private set; }

        /// <summary>
        /// Gets the list of WHEN expression.
        /// </summary>
        public ReadOnlyCollection<DbExpression> When { get; private set; }

        /// <summary>
        /// Gets the list of THEN expression.
        /// </summary>
        public ReadOnlyCollection<DbExpression> Then { get; private set; }

        /// <summary>
        /// Gets the ELSE expression.
        /// </summary>
        public DbExpression Else { get; private set; }

        /// <inheritdoc />
        public override void Accept(DbExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
