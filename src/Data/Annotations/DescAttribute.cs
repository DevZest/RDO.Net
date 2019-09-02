using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies sorting in descending order.
    /// </summary>
    public sealed class DescAttribute : SortAttribute
    {
        /// <inheritdoc />
        protected override SortDirection Direction
        {
            get { return SortDirection.Descending; }
        }
    }
}
