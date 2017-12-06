using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class DbCompositeIndexAttribute : ColumnsAttribute
    {
        public DbCompositeIndexAttribute(string name)
            : base(name)
        {
        }

        public string Description { get; set; }

        public bool IsUnique { get; set; }

        public bool IsClustered { get; set; }

        public bool IsMemberOfTable { get; set; } = true;

        public bool IsMemberOfTempTable { get; set; } = false;
    }
}
