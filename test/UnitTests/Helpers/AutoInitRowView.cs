using System.Windows;

namespace DevZest.Data.Windows.Helpers
{
    internal sealed class AutoInitRowView : RowView
    {
        internal override void Initialize(RowPresenter rowPresenter)
        {
            base.Initialize(rowPresenter);
            if (rowPresenter.ElementCollection == null)
                rowPresenter.InitElementPanel(null);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return RowPresenter.LayoutManager.MeasureRow(RowPresenter, constraint);
        }
    }
}
