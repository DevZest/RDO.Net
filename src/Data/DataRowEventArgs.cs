using System;

namespace DevZest.Data
{
    public class DataRowEventArgs : EventArgs
    {
        public DataRowEventArgs(DataRow dataRow)
        {
            dataRow.VerifyNotNull(nameof(dataRow));
            _dataRow = dataRow;
        }

        private readonly DataRow _dataRow;
        public DataRow DataRow
        {
            get { return _dataRow; }
        }
    }
}
