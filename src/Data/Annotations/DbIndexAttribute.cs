using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_DbIndexAttribute))]
    [NamedModelAttributeSpec(true, typeof(ColumnSort[]))]
    public sealed class DbIndexAttribute : DbIndexBaseAttribute
    {
        public DbIndexAttribute(string name)
            : base(name)
        {
        }

        protected override void Wireup(Model model, string dbName, ColumnSort[] sortOrder)
        {
            model.Index(dbName, Description, IsUnique, IsCluster, IsValidOnTable, IsValidOnTempTable, sortOrder);
        }

        public bool IsUnique { get; set; } = false;

        public bool IsValidOnTable { get; set; } = true;

        public bool IsValidOnTempTable { get; set; } = false;
    }
}
