using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class DbTableAttribute : DbTablePropertyAttribute
    {
        protected internal override void Wireup(Model model)
        {
            model.DbTableName = Name;
            model.DbTableDescription = Description;
        }

        public DbTableAttribute()
        {
        }

        public DbTableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
