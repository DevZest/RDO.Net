using System.Collections.Generic;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class DbCompositeIndexMemberAttribute : ColumnGroupMemberAttribute
    {
        private sealed class Manager : Manager<DbCompositeIndexAttribute, DbCompositeIndexMemberAttribute>
        {
            public static readonly Manager Singleton = new Manager();

            private Manager()
            {
            }

            protected override void Initialize(Model model, DbCompositeIndexAttribute columnsAttribute, IReadOnlyList<Entry> entries)
            {
                model.Index(columnsAttribute.Name, columnsAttribute.Description, columnsAttribute.IsUnique, columnsAttribute.IsClustered, columnsAttribute.IsMemberOfTable, columnsAttribute.IsMemberOfTempTable, GetOrderByList(entries));
            }

            private static ColumnSort[] GetOrderByList(IReadOnlyList<Entry> entries)
            {
                var result = new ColumnSort[entries.Count];
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var sortDirection = entry.MemberAttribute.SortDirection;
                    var column = entry.Column;
                    result[i] = sortDirection == SortDirection.Descending ? column.Desc() : column.Asc();
                }
                return result;
            }
        }

        public DbCompositeIndexMemberAttribute(string name)
            : base(name)
        {
        }

        protected override void Initialize(Column column)
        {
            Manager.Singleton.Initialize(this, column);
        }

        public SortDirection SortDirection { get; set; }
    }
}
