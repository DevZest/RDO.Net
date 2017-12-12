using DevZest.Data.Annotations.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.Annotations
{
    public sealed class CompositeUniqueAttribute : ValidationColumnGroupAttribute
    {
        public CompositeUniqueAttribute(string name)
            : base(name)
        {
        }

        public string Description { get; set; }

        public bool IsClustered { get; set; }

        protected override string GetDefaultMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            return Strings.UniqueColumnsAttribute_DefaultErrorMessage(columns);
        }
    }
}
