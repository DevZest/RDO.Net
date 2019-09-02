using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies sorting in ascending order.
    /// </summary>
    public sealed class AscAttribute : SortAttribute
    {
        /// <inheritdoc />
        protected override SortDirection Direction
        {
            get { return SortDirection.Ascending; }
        }
    }
}
