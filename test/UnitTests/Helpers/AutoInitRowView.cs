using DevZest.Data.Windows.Controls;
using System.Windows;

namespace DevZest.Data.Windows.Helpers
{
    internal sealed class AutoInitRowView : RowView
    {
        internal override void Setup(RowPresenter rowPresenter)
        {
            base.Setup(rowPresenter);
            if (Elements == null)
                Setup((FrameworkElement)null);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return RowPresenter.LayoutManager.MeasureRow(this, constraint);
        }
    }
}
