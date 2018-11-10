using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>Defines name of the column in the database.</summary>
    [ModelMemberAttributeSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column) }, ForceArgumentList = true)]
    public sealed class DbColumnAttribute : ColumnAttribute
    {
        public DbColumnAttribute()
        {
        }

        /// <summary>Initializes a new instance of <see cref="DbColumnAttribute"/> object.</summary>
        /// <param name="dbColumnName">The database column name.</param>
        public DbColumnAttribute(string dbColumnName)
        {
            Name = dbColumnName;
        }

        /// <summary>Gets or sets the column name for the column.</summary>
        public string Name { get; private set; }

        public string Description { get; set; }

        /// <inheritdoc/>
        protected sealed override void Wireup(Column column)
        {
            column.DbColumnName = Name;
            column.DbColumnDescription = Description;
        }
    }
}
