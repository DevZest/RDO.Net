using System;

namespace DevZest.Data
{
    /// <summary>Base class for attributes which can be decorated with a column.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public abstract class ColumnAttribute : Attribute
    {
        /// <summary>Initializes the provided <see cref="Column"/> object.</summary>
        /// <param name="column">The <see cref="Column"/> object.</param>
        protected internal abstract void Initialize(Column column);
    }
}
