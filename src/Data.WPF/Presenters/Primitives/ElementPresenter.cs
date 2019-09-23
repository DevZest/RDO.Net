namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Base class of <see cref="RowPresenter"/>, <see cref="ScalarPresenter"/> and <see cref="BlockPresenter"/>.
    /// </summary>
    public abstract class ElementPresenter
    {
        /// <summary>
        /// Initializes a new <see cref="ElementPresenter"/> class.
        /// </summary>
        protected ElementPresenter()
        {
        }

        /// <summary>
        /// Gets the <see cref="Template"/> associated with this element presenter.
        /// </summary>
        public abstract Template Template { get; }

        /// <summary>
        /// Gets the presenter which owns this element presenter.
        /// </summary>
        public BasePresenter Presenter
        {
            get { return Template.Presenter; }
        }

        /// <summary>
        /// Gets the data presenter.
        /// </summary>
        public DataPresenter DataPresenter
        {
            get { return Template?.DataPresenter; }
        }
    }
}
