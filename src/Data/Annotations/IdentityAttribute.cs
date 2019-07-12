using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int32) })]
    public sealed class IdentityAttribute : ColumnAttribute
    {
        public  IdentityAttribute()
            : this(1, 1)
        {
        }

        public IdentityAttribute(int seed, int increment)
        {
            Seed = seed;
            Increment = increment;
        }

        public int Seed { get; private set; }

        public int Increment { get; private set; }

        protected sealed override void Wireup(Column column)
        {
            if (column is _Int32 int32Column)
                int32Column.SetIdentity(Seed, Increment);
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
