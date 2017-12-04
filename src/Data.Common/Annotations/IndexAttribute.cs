using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IndexAttribute : ColumnAttribute
    {
        public IndexAttribute(string name)
        {
            Check.NotEmpty(name, nameof(name));
            Name = name;
        }

        protected override void Initialize(Column column)
        {
            column.ParentModel.Index(Name, IsUnique, IsCluster, IsMemberOfTable, IsMemberOfTempTable, SortDirection == SortDirection.Descending ? column.Desc() : column.Asc());
        }

        public string Name { get; private set; }

        public bool IsUnique { get; set; } = false;

        public bool IsCluster { get; set; }

        public bool IsMemberOfTable { get; set; } = true;

        public bool IsMemberOfTempTable { get; set; } = false;

        public SortDirection SortDirection { get; set; }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
