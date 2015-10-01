using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
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

        protected internal sealed override void Initialize(Column column)
        {
            ((_Int32)column).Identity(Seed, Increment);
        }
    }
}
