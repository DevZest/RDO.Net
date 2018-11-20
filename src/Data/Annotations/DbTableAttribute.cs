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

        public string Description { get; private set; }

        protected override void Initialize(PropertyInfo propertyInfo)
        {
        }

        protected override void Wireup<T>(DbTable<T> dbTable)
        {
            var model = dbTable.Model;
            model.DbTableName = Name;
            model.DbTableDescription = Description;
        }
    }
}
