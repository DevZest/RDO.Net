using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public class DataRowRemovedEventArgs : DataRowEventArgs
    {
        public DataRowRemovedEventArgs(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index)
            : base(dataRow)
        {
            baseDataSet.VerifyNotNull(nameof(baseDataSet));
            dataSet.VerifyNotNull(nameof(dataSet));

            _baseDataSet = baseDataSet;
            _ordinal = ordinal;
            _dataSet = dataSet;
            _index = index;
        }

        private readonly DataSet _baseDataSet;
        public DataSet BaseDataSet
        {
            get { return _baseDataSet; }
        }

        private readonly int _ordinal;
        public int Ordinal
        {
            get { return _ordinal; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        private readonly int _index;
        public int Index
        {
            get { return _index; }
        }
    }
}
