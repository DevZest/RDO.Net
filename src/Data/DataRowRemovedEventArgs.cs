namespace DevZest.Data
{
    /// <summary>
    /// Event data for <see cref="Model.DataRowRemoved"/> event.
    /// </summary>
    public class DataRowRemovedEventArgs : DataRowEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataRowRemovedEventArgs"/>.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <param name="baseDataSet">Base DataSet which the DataRow is removed from.</param>
        /// <param name="ordinal">The ordinal of the removed DataRow in base DataSet.</param>
        /// <param name="dataSet">DataSet which the DataRow is removed from.</param>
        /// <param name="index">The index of the removed DataRow in DataSet.</param>
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
        /// <summary>
        /// Gets the base DataSet which the DataRow is removed from.
        /// </summary>
        public DataSet BaseDataSet
        {
            get { return _baseDataSet; }
        }

        private readonly int _ordinal;
        /// <summary>
        /// Gets the ordinal of the removed DataRow in base DataSet.
        /// </summary>
        public int Ordinal
        {
            get { return _ordinal; }
        }

        private readonly DataSet _dataSet;
        /// <summary>
        /// Gets the DataSet which the DataRow is removed from.
        /// </summary>
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        private readonly int _index;
        /// <summary>
        /// Gets the index of the removed DataRow in DataSet.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }
    }
}
