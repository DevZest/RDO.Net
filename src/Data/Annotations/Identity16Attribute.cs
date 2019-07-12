using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int16) })]
    public sealed class Identity16Attribute : ColumnAttribute
    {
        public Identity16Attribute()
            : this(1, 1)
        {
        }

        public Identity16Attribute(short seed, short increment)
        {
            Seed = seed;
            Increment = increment;
        }

        public short Seed { get; private set; }

        public short Increment { get; private set; }

        protected sealed override void Wireup(Column column)
        {
            if (column is _Int16 int16Column)
                int16Column.SetIdentity(Seed, Increment);
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
