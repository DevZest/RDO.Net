using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AutoGuidAttribute : ColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            ((_Guid)column).SetDefault(Functions.NewGuid(), ConstraintName);
        }

        public string ConstraintName { get; set; }
    }
}
