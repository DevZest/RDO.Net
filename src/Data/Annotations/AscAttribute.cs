using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    public sealed class AscAttribute : SortAttribute
    {
        protected override SortDirection Direction
        {
            get { return SortDirection.Ascending; }
        }
    }
}
