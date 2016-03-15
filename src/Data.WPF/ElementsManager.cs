namespace DevZest.Data.Windows
{
    internal class ElementsManager
    {
        internal ElementsManager(DataPresenter dataPresenter)
        {
            DataPresenter = dataPresenter;
        }

        public DataPresenter DataPresenter { get; private set; }
    }
}
