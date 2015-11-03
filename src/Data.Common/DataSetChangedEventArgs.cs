using System;

namespace DevZest.Data
{
    public sealed class DataSetChangedEventArgs : EventArgs
    {
        internal static DataSetChangedEventArgs Create<T>(DataSet<T> dataSet, DataRow dataRow)
            where T : Model, new()
        {
            return new DataSetChangedEventArgs(dataSet.ParentRow, dataSet.Model, dataRow);
        }

        private DataSetChangedEventArgs(DataRow parentRow, Model model, DataRow dataRow)
        {
            ParentRow = parentRow;
            Model = model;
            DataRow = dataRow;
        }

        public DataRow ParentRow { get; private set; }

        public Model Model { get; private set; }

        public DataRow DataRow { get; private set; }
    }
}
