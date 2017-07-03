namespace DevZest.Data.Presenters.Primitives
{
    public interface IScalarElement
    {
        void Setup(ScalarPresenter scalarPresenter);
        void Refresh(ScalarPresenter scalarPresenter);
        void Cleanup(ScalarPresenter scalarPresenter);
    }
}
