using System;
using DevZest.Data;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtFlagAttribute : ColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            column.Nullable(false);
        }
    }
}
