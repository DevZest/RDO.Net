using System;

namespace DevZest.Data
{
    public sealed class ColumnValueChangedEventArgs : EventArgs
    {
        internal ColumnValueChangedEventArgs(DataSet dataSet, DataRow dataRow, Column column)
        {
            DataSet = dataSet;
            DataRow = dataRow;
            Column = column;
        }

        public DataSet DataSet { get; private set; }

        public DataRow DataRow { get; private set; }

        public Column Column { get; private set; }
    }
}
