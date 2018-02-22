namespace DevZest.Data.Presenters.Primitives
{
    public interface IScalarElement
    {
        void Setup(ScalarPresenter p);
        void Refresh(ScalarPresenter p);
        void Cleanup(ScalarPresenter p);
    }
}
