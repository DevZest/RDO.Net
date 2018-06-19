using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AutoGuidAttribute : ColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            ((_Guid)column).SetDefault(Functions.NewGuid(), Name, Description);
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
