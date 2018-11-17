namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ElementPresenter
    {
        protected ElementPresenter()
        {
        }

        public abstract Template Template { get; }

        public BasePresenter Presenter
        {
            get { return Template.Presenter; }
        }

        public DataPresenter DataPresenter
        {
            get { return Template?.DataPresenter; }
        }
    }
}
