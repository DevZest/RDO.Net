namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Interface to indicate <see cref="IService"/> is reloadable.
    /// </summary>
    public interface IReloadableService
    {
    }

    /// <summary>
    /// Base interface to provide customizable presentation logic for view components.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Gets the associated <see cref="DataPresenter"/>.
        /// </summary>
        DataPresenter DataPresenter { get; }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <param name="dataPresenter">The <see cref="DataPresenter"/>.</param>
        void Initialize(DataPresenter dataPresenter);
    }
}
