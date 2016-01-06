namespace DevZest.Data
{
    public class DataRowRemovedEventArgs : DataRowEventArgs
    {
        internal DataRowRemovedEventArgs(DataRow dataRow)
            : base(dataRow)
        {
        }
    }
}
