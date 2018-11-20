using System.Reflection;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class DbTableAttribute : DbTablePropertyAttribute
    {
        public DbTableAttribute()
        {
        }

        public DbTableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string Description { get; set; }

        protected override void Initialize(PropertyInfo propertyInfo)
        {
        }

        protected override void Wireup<T>(DbTable<T> dbTable)
        {
            dbTable.Name = Name;
            dbTable.Description = Description;
        }
    }
}
