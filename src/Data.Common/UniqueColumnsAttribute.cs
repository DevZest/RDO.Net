using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data
{
    public sealed class UniqueColumnsAttribute : ValidatorColumnsAttribute
    {
        public UniqueColumnsAttribute(string name)
            : base(name)
        {
        }

        public bool IsClustered { get; set; }

        protected override string GetDefaultMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            return Strings.UniqueColumnsAttribute_DefaultErrorMessage(columns);
        }
    }
}
