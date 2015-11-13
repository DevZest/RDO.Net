using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class DataSource
    {
        internal DataSource(Model model)
        {
            Debug.Assert(model != null);
            Model = model;
        }

        public Model Model { get; private set; }

        public abstract DataSourceKind Kind { get; }

        internal int Revision { get; private set; }

        internal void UpdateRevision()
        {
            Revision++;
        }

        private DataSource ChildSource
        {
            get
            {
                var dbTable = this as IDbTable;
                if (dbTable == null)
                    return null;

                var source = dbTable.Source;
                var result = source.DataSource;
                return result == null || result.Revision != source.Revision ? null : result;
            }
        }

        internal DataSource Source
        {
            get
            {
                var result = this;
                for (var childSource = ChildSource; childSource != null;)
                    result = childSource;
                return result;
            }
        }
    }
}
