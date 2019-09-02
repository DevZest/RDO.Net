using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies <see cref="Int16"/> identity column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(Identity) }, validOnTypes: new Type[] { typeof(_Int16) })]
    public sealed class Identity16Attribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Identity16Attribute"/>.
        /// </summary>
        public Identity16Attribute()
            : this(1, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Identity16Attribute"/> with specified seed and increment.
        /// </summary>
        /// <param name="seed">The first value of the identity column.</param>
        /// <param name="increment">The increment each time a new value generated from the identity column.</param>
        public Identity16Attribute(short seed, short increment)
        {
            Seed = seed;
            Increment = increment;
        }

        /// <summary>
        /// Gets the first value of the identity column.
        /// </summary>
        public short Seed { get; private set; }

        /// <summary>
        /// Gets the increment each time a new value gnerated from the identity column.
        /// </summary>
        public short Increment { get; private set; }

        /// <inheritdoc />
        protected sealed override void Wireup(Column column)
        {
            if (column is _Int16 int16Column)
                int16Column.SetIdentity(Seed, Increment);
        }

        /// <inheritdoc />
        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
