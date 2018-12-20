using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int64) })]
    public sealed class Identity64Attribute : ColumnAttribute
    {
        public Identity64Attribute(long seed, long increment)
        {
            Seed = seed;
            Increment = increment;
        }

        public long Seed { get; private set; }

        public long Increment { get; private set; }

        protected sealed override void Wireup(Column column)
        {
            if (column is _Int64 int64Column)
                int64Column.SetIdentity(Seed, Increment);
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
