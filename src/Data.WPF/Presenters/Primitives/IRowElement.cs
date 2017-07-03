namespace DevZest.Data.Presenters.Primitives
{
    public interface IRowElement
    {
        void Setup(RowPresenter rowPresenter);
        void Refresh(RowPresenter rowPresenter);
        void Cleanup(RowPresenter rowPresenter);
    }
}
