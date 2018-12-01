using DevZest.Data;
using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnNotNull) }, validOnTypes: new Type[] { typeof(_Boolean) })]
    public sealed class UdtNameStyleAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Boolean boolean)
                boolean.Nullable(false);
        }
    }
}
