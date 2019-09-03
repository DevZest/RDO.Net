using System;

namespace DevZest.Data
{
    /// <summary>
    /// Event data which contains a <see cref="DataRow"/> object.
    /// </summary>
    public class DataRowEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataRowEventArgs"/>.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        public DataRowEventArgs(DataRow dataRow)
        {
            dataRow.VerifyNotNull(nameof(dataRow));
            _dataRow = dataRow;
        }

        private readonly DataRow _dataRow;
        /// <summary>
        /// Gets the <see cref="DataRow"/> object.
        /// </summary>
        public DataRow DataRow
        {
            get { return _dataRow; }
        }
    }
}
