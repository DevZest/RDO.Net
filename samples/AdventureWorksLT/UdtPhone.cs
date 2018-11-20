using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Addons;
using DevZest.Data.SqlServer.Addons;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(ColumnNotNull), typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class UdtPhone : ColumnAttribute
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
