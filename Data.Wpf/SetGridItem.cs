namespace DevZest.Data.Windows
{
    public abstract class SetGridItem : GridItem
    {
        protected SetGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        internal virtual GridTemplate Template
        {
            get { return null; }
        }
    }
}
