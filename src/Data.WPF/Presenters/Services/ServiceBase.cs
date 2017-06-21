namespace DevZest.Data.Presenters.Services
{
    internal abstract class ServiceBase : IService
    {
        public DataPresenter DataPresenter { get; set; }
    }
}
