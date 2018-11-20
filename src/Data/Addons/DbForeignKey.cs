using System;
using System.Diagnostics;

namespace DevZest.Data.Addons
{
    public sealed class DbForeignKey : DbTableConstraint
    {
        internal DbForeignKey(string name, string description, PrimaryKey foreignKey, PrimaryKey referencedKey, ForeignKeyAction onDelete, ForeignKeyAction onUpdate)
            : base(name, description)
        {
            Debug.Assert(foreignKey != null);
            Debug.Assert(referencedKey != null);
            Debug.Assert(foreignKey.GetType() == referencedKey.GetType());

            ForeignKey = foreignKey;
            ReferencedKey = referencedKey;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
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

        public ForeignKeyAction OnDelete { get; private set; }

        public ForeignKeyAction OnUpdate { get; private set; }

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
