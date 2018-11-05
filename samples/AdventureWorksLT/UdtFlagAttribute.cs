using DevZest.Data;
using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    [ModelMemberAttributeSpec(typeof(ColumnNotNull), true, typeof(_Boolean))]
    public sealed class UdtFlagAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Boolean boolean)
                boolean.Nullable(false);
        }
    }
}
