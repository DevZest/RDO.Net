using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the column has a default value of current <see cref="DateTime"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnDefault) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class AutoDateTimeAttribute : ColumnAttribute
    {
        /// <inheritdoc />
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.SetDefault(IsUtc ? _DateTime.UtcNow() : _DateTime.Now(), Name, Description);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value is UTC.
        /// </summary>
        public bool IsUtc { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }
    }
}
