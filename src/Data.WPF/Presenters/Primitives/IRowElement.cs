namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents view element that can be used as row binding target.
    /// </summary>
    public interface IRowElement
    {
        /// <summary>
        /// Setup this view element with row presenter.
        /// </summary>
        /// <param name="p">The row presenter.</param>
        void Setup(RowPresenter p);

        /// <summary>
        /// Refresh this view element with row presenter.
        /// </summary>
        /// <param name="p">The row presenter.</param>
        void Refresh(RowPresenter p);

        /// <summary>
        /// Cleanup this view element with row presenter.
        /// </summary>
        /// <param name="p">The row presenter.</param>
        void Cleanup(RowPresenter p);
    }
}
