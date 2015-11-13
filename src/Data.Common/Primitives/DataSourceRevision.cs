namespace DevZest.Data.Primitives
{
    internal struct DataSourceRevision
    {
        public DataSourceRevision(DataSource dataSource)
            : this(dataSource, dataSource == null ? -1 : dataSource.Revision)
        {
        }

        private DataSourceRevision(DataSource dataSource, int revision)
        {
            DataSource = dataSource;
            Revision = revision;
        }

        public readonly DataSource DataSource;

        public readonly int Revision;

        public bool IsEmpty
        {
            get { return DataSource == null && Revision == 0; }
        }
    }
}
