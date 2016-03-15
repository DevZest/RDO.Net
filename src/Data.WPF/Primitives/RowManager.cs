namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowManager
    {
        internal RowManager(DataSet dataSet)
        {
            _template = new Template();
            _dataSet = dataSet;
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        public Model Model
        {
            get { return DataSet.Model; }
        }

        internal virtual void Initialize()
        {

        }
    }
}
