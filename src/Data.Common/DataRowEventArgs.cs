using System;

namespace DevZest.Data
{
    public class DataRowEventArgs : EventArgs
    {
        internal DataRowEventArgs(DataRow dataRow)
        {
            DataRow = dataRow;
        }

        public DataRow DataRow { get; private set; }
    }
}
