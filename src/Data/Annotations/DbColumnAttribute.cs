using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>Specifies name and/or description of the column in the database.</summary>
    [ModelDesignerSpec(addonTypes: null, validOnTypes: new Type[] { typeof(Column) }, RequiresArgument = true)]
    public sealed class DbColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbColumnAttribute"/>.
        /// </summary>
        public DbColumnAttribute()
        {
        }

        /// <summary>Initializes a new instance of <see cref="DbColumnAttribute"/> object.</summary>
        /// <param name="dbColumnName">The database column name.</param>
        public DbColumnAttribute(string dbColumnName)
        {
            Name = dbColumnName;
        }

        /// <summary>Gets or sets the name of the column in the database.</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description of the column in the database.
        /// </summary>
        public string Description { get; set; }

        /// <inheritdoc/>
        protected sealed override void Wireup(Column column)
        {
            column.DbColumnName = Name;
            column.DbColumnDescription = Description;
        }
    }
}
