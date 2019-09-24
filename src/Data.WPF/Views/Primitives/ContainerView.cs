using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows.Controls;

namespace DevZest.Data.Views.Primitives
{
    /// <summary>
    /// Base class of <see cref="BlockView"/> and <see cref="RowView"/>.
    /// </summary>
    public abstract class ContainerView : Control
    {
        /// <summary>
        /// Gets the ordinal of the container.
        /// </summary>
        public abstract int ContainerOrdinal { get; }

        internal abstract ElementManager ElementManager { get; }

        internal ScrollableManager ScrollableManager
        {
            get { return ElementManager as ScrollableManager; }
        }

        internal abstract void Setup(ElementManager elementManager, int containerOrdinal);

        internal abstract void ReloadCurrentRow(RowPresenter oldValue);

        internal abstract bool AffectedOnRowsChanged { get; }

        internal double[] CumulativeMeasuredLengths { get; set; }

        internal double StartOffset { get; set; }

        internal abstract void Refresh();

        internal abstract void Cleanup();
    }
}
