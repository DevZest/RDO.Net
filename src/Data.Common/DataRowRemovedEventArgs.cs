namespace DevZest.Data
{
    public class DataRowRemovedEventArgs : DataRowEventArgs
    {
        internal DataRowRemovedEventArgs(DataRow dataRow)
            : base(dataRow)
        {
            Model = dataRow.Model;
            ParentDataRow = dataRow.ParentDataRow;
            Ordinal = dataRow.Ordinal;
            Index = dataRow.Index;
        }

        public Model Model { get; private set; }

        public DataRow ParentDataRow { get; private set; }

        public int Ordinal { get; private set; }

        public int Index { get; private set; }

        public DataSet DataSet
        {
            get { return ParentDataRow == null ? GlobalDataSet : ParentDataRow[Model]; }
        }

        public DataSet GlobalDataSet
        {
            get { return DataSet.FromModel(Model); }
        }
    }
}
