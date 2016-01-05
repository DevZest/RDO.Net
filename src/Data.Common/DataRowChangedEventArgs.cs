using System;

namespace DevZest.Data
{
    public sealed class DataRowChangedEventArgs : EventArgs
    {
        internal DataRowChangedEventArgs(DataRow dataRow)
        {
            DataRow = dataRow;
        }

        public DataRow DataRow { get; private set; }
    }
}
