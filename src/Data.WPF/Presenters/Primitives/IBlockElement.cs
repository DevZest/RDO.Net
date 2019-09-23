namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents view element that can be used as block binding target.
    /// </summary>
    public interface IBlockElement
    {
        /// <summary>
        /// Setup this view element with block presenter.
        /// </summary>
        /// <param name="p">The block presenter.</param>
        void Setup(BlockPresenter p);

        /// <summary>
        /// Refresh this view element with block presenter.
        /// </summary>
        /// <param name="p">The block presenter.</param>
        void Refresh(BlockPresenter p);

        /// <summary>
        /// Cleanup this view element with block presenter.
        /// </summary>
        /// <param name="p">The block presenter.</param>
        void Cleanup(BlockPresenter p);
    }
}
