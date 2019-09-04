using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents SQL JOIN clause.
    /// </summary>
    public sealed class DbJoinClause : DbFromClause
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbJoinClause"/> class.
        /// </summary>
        /// <param name="kind">The operation of SQL JOIN.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <param name="on">The SQL JOIN column mappings.</param>
        public DbJoinClause(DbJoinKind kind, DbFromClause left, DbFromClause right, IReadOnlyList<ColumnMapping> on)
        {
            Kind = kind;
            Left = left;
            Right = right;
            On = on;
        }

        /// <summary>
        /// Gets the operation of SQL JOIN.
        /// </summary>
        public DbJoinKind Kind { get; private set; }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        public DbFromClause Left { get; private set; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        public DbFromClause Right { get; private set; }

        /// <summary>
        /// Gets the SQL JOIN column mappings.
        /// </summary>
        public IReadOnlyList<ColumnMapping> On { get; private set; }

        /// <inheritdoc />
        public override void Accept(DbFromClauseVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public override T Accept<T>(DbFromClauseVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override void OnClone(Model model)
        {
        }
    }
}
