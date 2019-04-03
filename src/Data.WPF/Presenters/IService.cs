namespace DevZest.Data.Presenters
{
    public interface IReloadableService
    {
    }

    public interface IService
    {
        DataPresenter DataPresenter { get; }
        void Initialize(DataPresenter dataPresenter);
    }
}
