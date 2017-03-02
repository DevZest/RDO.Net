using DevZest.Windows.Data.Primitives;
using System.Windows.Controls;

namespace DevZest.Windows.Data.Primitives
{
    public abstract class ContainerView : Control
    {
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
