using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class ForeignKeyConstraint : DbTableConstraint
    {
        internal ForeignKeyConstraint(string name, KeyBase foreignKey, KeyBase referencedKey, ForeignKeyAction onDelete, ForeignKeyAction onUpdate)
            : base(name)
        {
            Debug.Assert(foreignKey != null);
            Debug.Assert(referencedKey != null);
            Debug.Assert(foreignKey.GetType() == referencedKey.GetType());

            ForeignKey = foreignKey;
            ReferencedKey = referencedKey;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
        }

        public KeyBase ForeignKey { get; private set; }

        public string ReferencedTableName
        {
            get
            {
                var dbTable = ReferencedKey.ParentModel.DataSource as IDbTable;
                return dbTable == null ? null : dbTable.Name;
            }
        }

        public KeyBase ReferencedKey { get; private set; }

        public ForeignKeyAction OnDelete { get; private set; }

        public ForeignKeyAction OnUpdate { get; private set; }
    }
}
