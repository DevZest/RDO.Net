using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the column of a default value of new GUID.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnDefault) }, validOnTypes: new Type[] { typeof(_Guid) })]
    public sealed class AutoGuidAttribute : ColumnAttribute
    {
        /// <inheritdoc />
        protected override void Wireup(Column column)
        {
            if (column is _Guid guidColumn)
                guidColumn.SetDefault(_Guid.NewGuid(), Name, Description);
        }

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
