namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents view element that can be used as scalar binding target.
    /// </summary>
    public interface IScalarElement
    {
        /// <summary>
        /// Setup this view element with scalar presenter.
        /// </summary>
        /// <param name="p">The scalar presenter.</param>
        void Setup(ScalarPresenter p);

        /// <summary>
        /// Refresh this view element with scalar presenter.
        /// </summary>
        /// <param name="p">The scalar presenter.</param>
        void Refresh(ScalarPresenter p);

        /// <summary>
        /// Cleanup this view element with scalar presenter.
        /// </summary>
        /// <param name="p">The scalar presenter.</param>
        void Cleanup(ScalarPresenter p);
    }
}
