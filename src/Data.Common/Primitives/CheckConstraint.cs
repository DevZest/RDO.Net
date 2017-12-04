using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents database table CHECK constraint.</summary>
    public sealed class CheckConstraint : DbTableConstraint
    {
        internal CheckConstraint(string name, DbExpression logicalExpression)
            : base(name)
        {
            LogicalExpression = logicalExpression;
        }

        /// <summary>Gets the logical expression of this CHECK constraint.</summary>
        public DbExpression LogicalExpression { get; private set; }

        public override bool IsMemberOfTable
        {
            get { return true; }
        }

        public override bool IsMemberOfTempTable
        {
            get { return true; }
        }
    }
}
