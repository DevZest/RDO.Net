namespace DevZest.Data.Presenters.Primitives
{
    public interface IRowElement
    {
        void Setup(RowPresenter p);
        void Refresh(RowPresenter p);
        void Cleanup(RowPresenter p);
    }
}
