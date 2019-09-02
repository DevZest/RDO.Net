using System.Reflection;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the database table.
    /// </summary>
    public sealed class DbTableAttribute : DbTablePropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbTableAttribute"/>.
        /// </summary>
        public DbTableAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DbTableAttribute"/> by specified name.
        /// </summary>
        /// <param name="name"></param>
        public DbTableAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the database table.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description of the database table.
        /// </summary>
        public string Description { get; set; }

        /// <inheritdoc />
        protected override void Initialize(PropertyInfo propertyInfo)
        {
        }

        /// <inheritdoc />
        protected override void Wireup<T>(DbTable<T> dbTable)
        {
            dbTable.Name = Name;
            dbTable.Description = Description;
        }
    }
}
