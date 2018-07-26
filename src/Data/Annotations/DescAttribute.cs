using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class DescAttribute : SortAttribute
    {
        protected override SortDirection Direction
        {
            get { return SortDirection.Descending; }
        }
    }
}
