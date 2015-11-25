using System;

namespace DevZest.Data
{
    public sealed class RowCollectionChangedEventArgs : EventArgs
    {
        internal RowCollectionChangedEventArgs(DataSet dataSet, int oldIndex, DataRow dataRow)
        {
            DataSet = dataSet;
            OldIndex = oldIndex;
            DataRow = dataRow;
        }

        public DataSet DataSet { get; private set; }

        public int OldIndex { get; private set; }

        public DataRow DataRow { get; private set; }
    }
}
