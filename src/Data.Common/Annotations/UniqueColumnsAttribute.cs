using DevZest.Data.Annotations.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.Annotations
{
    public sealed class UniqueColumnsAttribute : ValidationColumnsAttribute
    {
        public UniqueColumnsAttribute(string name)
            : base(name)
        {
        }

        public bool IsClustered { get; set; }

        public bool IsMemberOfTable { get; set; } = true;

        public bool IsMemberOfTempTable { get; set; } = true;

        protected override string GetDefaultMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            return Strings.UniqueColumnsAttribute_DefaultErrorMessage(columns);
        }
    }
}
