using System.Diagnostics;

namespace DevZest.Data.Addons
{
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

        public CandidateKey ForeignKey { get; private set; }

        public string ReferencedTableName
        {
            get
            {
                var dbTable = ReferencedKey.ParentModel.DataSource as IDbTable;
                return dbTable?.Name;
            }
        }

        public CandidateKey ReferencedKey { get; private set; }

        public ForeignKeyRule DeleteRule { get; private set; }

        public ForeignKeyRule UpdateRule { get; private set; }

        public override bool IsValidOnTable
        {
            get { return true; }
        }

        public override bool IsValidOnTempTable
        {
            get { return false; }
        }
    }
}
