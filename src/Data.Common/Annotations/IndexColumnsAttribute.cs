using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class IndexColumnsAttribute : ColumnsAttribute
    {
        public IndexColumnsAttribute(string name)
            : base(name)
        {
        }

        public bool IsUnique { get; set; }

        public bool IsClustered { get; set; }

        public bool IsMemberOfTable { get; set; } = true;

        public bool IsMemberOfTempTable { get; set; } = false;
    }
}
