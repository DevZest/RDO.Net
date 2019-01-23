using DevZest.Data;
using DevZest.Data.MySql;
using DevZest.Data.Annotations.Primitives;
using System;
using DevZest.Data.Addons;
using DevZest.Data.MySql.Addons;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnNotNull), typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class UdtOrderNumber : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
            {
                stringColumn.Nullable(true);
                stringColumn.AsMySqlNVarChar(25);
            }
        }
    }
}
