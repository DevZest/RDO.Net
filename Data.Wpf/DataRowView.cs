using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public sealed class DataRowView
    {
        internal DataRowView(DataSetView owner, DataRow dataRow)
        {
            Debug.Assert(owner != null);
            Debug.Assert(dataRow != null && owner.Model == dataRow.Model);
            Owner = owner;
            DataRow = dataRow;
        }

        public DataSetView Owner { get; private set; }

        public DataRow DataRow { get; private set; }

        public Model Model
        {
            get { return Owner.Model; }
        }
    }
}
