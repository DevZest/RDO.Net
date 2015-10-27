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
    }
}
