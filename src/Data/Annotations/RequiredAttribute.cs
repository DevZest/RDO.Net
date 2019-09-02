using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies that a data column is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column) })]
    public sealed class RequiredAttribute : ValidationColumnAttribute
    {
        /// <inheritdoc />
        protected override void Wireup(Column column)
        {
            base.Wireup(column);
            column.Nullable(false);
        }

        /// <inheritdoc />
        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return !column.IsNull(dataRow);
        }

        /// <inheritdoc />
        protected override string DefaultMessageString
        {
            get { return UserMessages.RequiredAttribute; }
        }

        /// <inheritdoc />
        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
