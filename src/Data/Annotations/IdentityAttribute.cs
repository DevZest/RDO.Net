using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
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
            ((_Int32)column).Identity(Seed, Increment);
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
