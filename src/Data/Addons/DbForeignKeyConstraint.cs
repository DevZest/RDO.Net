using System.Diagnostics;

namespace DevZest.Data.Addons
{
    public sealed class DbForeignKeyConstraint : DbTableConstraint
    {
        internal DbForeignKeyConstraint(string name, string description, PrimaryKey foreignKey, PrimaryKey referencedKey, Rule deleteRule, Rule updateRule)
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

        public PrimaryKey ForeignKey { get; private set; }

        public string ReferencedTableName
        {
            get
            {
                var dbTable = ReferencedKey.ParentModel.DataSource as IDbTable;
                return dbTable?.Name;
            }
        }

        public PrimaryKey ReferencedKey { get; private set; }

        public Rule DeleteRule { get; private set; }

        public Rule UpdateRule { get; private set; }

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
