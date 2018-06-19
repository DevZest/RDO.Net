using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AutoDateTimeAttribute : ColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            ((_DateTime)column).SetDefault(Functions.GetDate(), Name, Description);
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
