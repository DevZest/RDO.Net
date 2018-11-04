using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations.Primitives;
using System;
using DevZest.Data.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelMemberAttributeSpec(new Type[] { typeof(NotNull), typeof(SqlColumnDescriptor) }, true, typeof(_String))]
    public sealed class UdtOrderNumber : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
            {
                stringColumn.Nullable(true);
                stringColumn.AsSqlNVarChar(25);
            }
        }
    }
}
