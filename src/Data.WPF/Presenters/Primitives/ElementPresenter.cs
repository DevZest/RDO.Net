namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ElementPresenter
    {
        protected ElementPresenter()
        {
        }

        public abstract Template Template { get; }

        public DataPresenter DataPresenter
        {
            get { return Template?.DataPresenter; }
        }
    }
}
