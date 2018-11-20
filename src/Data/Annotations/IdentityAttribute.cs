using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [AttributeSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int32), typeof(_Int64), typeof(_Int16) })]
    public sealed class IdentityAttribute : ColumnAttribute
    {
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
            else if (column is _Int64 int64Column)
                int64Column.SetIdentity(Seed, Increment);
            else if (column is _Int16 int16Column)
                int16Column.SetIdentity(Seed, Increment);
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
