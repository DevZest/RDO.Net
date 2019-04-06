using DevZest.Data.Primitives;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents database table CHECK constraint.
    /// </summary>
    public sealed class DbCheckConstraint : DbTableConstraint
    {
        internal DbCheckConstraint(string name, string description, DbExpression logicalExpression)
            : base(name, description)
        {
            LogicalExpression = logicalExpression;
        }

        /// <summary>Gets the logical expression of this CHECK constraint.</summary>
        public DbExpression LogicalExpression { get; private set; }

        /// <inheritdoc />
        public override bool IsValidOnTable
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsValidOnTempTable
        {
            get { return true; }
        }
    }
}
