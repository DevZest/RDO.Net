using System;

namespace DevZest.Data
{
    public class DataRowRemovedEventArgs : EventArgs
    {
        internal DataRowRemovedEventArgs(DataRow dataRow)
        {
            DataRow = dataRow;
            Model = dataRow.Model;
            ParentDataRow = dataRow.ParentDataRow;
            Ordinal = dataRow.Ordinal;
            Index = dataRow.Index;
        }

        public DataRow DataRow { get; private set; }

        public Model Model { get; private set; }

        public DataRow ParentDataRow { get; private set; }

        public int Ordinal { get; private set; }

        public int Index { get; private set; }

        public DataSet DataSet
        {
            get { return ParentDataRow == null ? BaseDataSet : ParentDataRow[Model]; }
        }

        public DataSet BaseDataSet
        {
            get { return Model.DataSet; }
        }
    }
}
