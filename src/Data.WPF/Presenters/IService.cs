namespace DevZest.Data.Presenters
{
    public interface IService
    {
        DataPresenter DataPresenter { get; }
        void Initialize(DataPresenter dataPresenter);
    }
}
