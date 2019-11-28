namespace DevZest.Data.Tools
{
    public abstract class IndexWindowBase : RowArrangerDialogWindow
    {
        protected IndexWindowBase()
        {
        }

        protected override void OnRowArranged()
        {
            ((IndexPresenterBase)GetPresenter()).RefreshName();
        }
    }
}
