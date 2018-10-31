using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtAccountNumberAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            column.Nullable(true);
            ((Column<string>)column).AsNVarChar(15);
        }
    }
}
