using DevZest.Data.Windows.Primitives;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public abstract class ContainerView : Control
    {
        public abstract int ContainerOrdinal { get; }

        internal abstract ElementManager ElementManager { get; }

        internal LayoutScrollableManager LayoutXYManager
        {
            get { return ElementManager as LayoutScrollableManager; }
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
