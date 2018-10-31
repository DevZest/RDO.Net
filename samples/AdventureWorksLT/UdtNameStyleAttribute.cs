using System;
using DevZest.Data;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtNameStyleAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            column.Nullable(false);
        }
    }
}
