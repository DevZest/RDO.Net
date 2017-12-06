using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DbIndexAttribute : ColumnAttribute
    {
        public DbIndexAttribute(string name)
        {
            Check.NotEmpty(name, nameof(name));
            Name = name;
        }

        protected override void Initialize(Column column)
        {
            column.ParentModel.Index(Name, Description, IsUnique, IsCluster, IsMemberOfTable, IsMemberOfTempTable, SortDirection == SortDirection.Descending ? column.Desc() : column.Asc());
        }

        public string Name { get; private set; }

        public string Description { get; set; }

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
