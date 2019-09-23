using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents a behavior that can dynamically change the look-and-feel of <see cref="RowView"/>.
    /// </summary>
    public abstract class RowViewBehavior
    {
        /// <summary>
        /// Setup the <see cref="RowView"/>.
        /// </summary>
        /// <param name="rowView">The <see cref="RowView"/>.</param>
        protected internal abstract void Setup(RowView rowView);

        /// <summary>
        /// Refresh the <see cref="RowView"/>.
        /// </summary>
        /// <param name="rowView">The <see cref="RowView"/>.</param>
        protected internal abstract void Refresh(RowView rowView);

        /// <summary>
        /// Cleanup the <see cref="RowView"/>.
        /// </summary>
        /// <param name="rowView">The <see cref="RowView"/>.</param>
        protected internal abstract void Cleanup(RowView rowView);
    }
}
