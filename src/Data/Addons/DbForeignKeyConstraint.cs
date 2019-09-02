using System.Diagnostics;

namespace DevZest.Data.Addons
{
    /// <summary>
    /// Represents database table foreign key constraint.
    /// </summary>
    public sealed class DbForeignKeyConstraint : DbTableConstraint
    {
        internal DbForeignKeyConstraint(string name, string description, CandidateKey foreignKey, CandidateKey referencedKey, ForeignKeyRule deleteRule, ForeignKeyRule updateRule)
            : base(name, description)
        {
            Debug.Assert(foreignKey != null);
            Debug.Assert(referencedKey != null);
            Debug.Assert(foreignKey.GetType() == referencedKey.GetType());

            ForeignKey = foreignKey;
            ReferencedKey = referencedKey;
            DeleteRule = deleteRule;
            UpdateRule = updateRule;
        }

        /// <summary>
        /// Gets the foreign key of this constraint.
        /// </summary>
        public CandidateKey ForeignKey { get; private set; }

        /// <summary>
        /// Gets the referenced table name of this constraint.
        /// </summary>
        public string ReferencedTableName
        {
            get
            {
                var dbTable = ReferencedKey.ParentModel.DataSource as IDbTable;
                return dbTable?.Name;
            }
        }

        /// <summary>
        /// Gets the referenced key of this constraint.
        /// </summary>
        public CandidateKey ReferencedKey { get; private set; }

        /// <summary>
        /// Gets the foreign key rule when rows are deleted.
        /// </summary>
        public ForeignKeyRule DeleteRule { get; private set; }

        /// <summary>
        /// Gets the foreign key rule when rows are updated.
        /// </summary>
        public ForeignKeyRule UpdateRule { get; private set; }

        /// <inheritdoc />
        public override bool IsValidOnTable
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsValidOnTempTable
        {
            get { return false; }
        }
    }
}
