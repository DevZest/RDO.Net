namespace DevZest.Data.Windows.Helpers
{
    internal sealed class AutoInitRowView : RowView
    {
        internal override void Initialize(RowPresenter rowPresenter)
        {
            base.Initialize(rowPresenter);
            rowPresenter.InitElementPanel(null);
        }
    }
}
