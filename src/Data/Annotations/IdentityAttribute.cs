using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies <see cref="Int32"/> identity column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int32) })]
    public sealed class IdentityAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IdentityAttribute"/>.
        /// </summary>
        public  IdentityAttribute()
            : this(1, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IdentityAttribute"/> with specified seed and increment.
        /// </summary>
        /// <param name="seed">The first value of the identity column.</param>
        /// <param name="increment">The increment each time a new value generated from the identity column.</param>
        public IdentityAttribute(int seed, int increment)
        {
            Seed = seed;
            Increment = increment;
        }

        /// <summary>
        /// Gets the first value of the identity column.
        /// </summary>
        public int Seed { get; private set; }

        /// <summary>
        /// Gets the increment each time a new value gnerated from the identity column.
        /// </summary>
        public int Increment { get; private set; }

        /// <inheritdoc />
        protected sealed override void Wireup(Column column)
        {
            if (column is _Int32 int32Column)
                int32Column.SetIdentity(Seed, Increment);
        }

        /// <inheritdoc />
        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
