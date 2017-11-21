using System.Collections.Generic;
using DevZest.Data.Primitives;
using System.Diagnostics;
using System;

namespace DevZest.Data
{
    public sealed class UniqueColumnsMemberAttribute : ValidatorColumnsMemberAttribute
    {
        private sealed class Manager : Manager<UniqueColumnsAttribute, UniqueColumnsMemberAttribute>
        {
            public static readonly Manager Singleton = new Manager();

            private Manager()
            {
            }

            protected override void Initialize(Model model, UniqueColumnsAttribute columnsAttribute, IReadOnlyList<Entry> entries)
            {
                base.Initialize(model, columnsAttribute, entries);
                model.Unique(columnsAttribute.Name, columnsAttribute.IsClustered, GetOrderByList(entries));
            }

            protected override bool IsValid(IValidationContext validationContext, DataRow dataRow)
            {
                var dataSet = validationContext.Model.DataSet;
                foreach (var other in dataSet)
                {
                    if (dataRow != other && AreEqual(validationContext.ColumnList, dataRow, other))
                        return false;
                }
                return true;
            }

            private bool AreEqual(IReadOnlyList<Column> columnList, DataRow dataRow, DataRow other)
            {
                Debug.Assert(dataRow != other);
                for (int i = 0; i < columnList.Count; i++)
                {
                    if (columnList[i].Compare(other, dataRow, SortDirection.Unspecified) != 0)
                        return false;
                }
                return true;
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

        public UniqueColumnsMemberAttribute(string name)
            : base(name)
        {
        }

        protected internal override void Initialize(Column column)
        {
            Manager.Singleton.Initialize(this, column);
        }

        public SortDirection SortDirection { get; set; }
    }
}
