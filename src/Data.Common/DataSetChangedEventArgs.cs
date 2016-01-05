using System;

namespace DevZest.Data
{
    public sealed class DataSetChangedEventArgs : EventArgs
    {
        internal DataSetChangedEventArgs(DataSetChangedAction action, int ordinal, DataRow dataRow)
        {
            Action = action;
            Ordinal = ordinal;
            DataRow = dataRow;
        }

        public DataSetChangedAction Action { get; private set; }

        public int Ordinal { get; private set; }

        public DataRow DataRow { get; private set; }

    }
}
