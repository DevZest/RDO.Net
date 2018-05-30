namespace DevZest.Data
{
    public class ValueChangedEventArgs : DataRowEventArgs
    {
        public ValueChangedEventArgs(DataRow dataRow, IColumns columns)
            : base(dataRow)
        {
            columns.VerifyNotNull(nameof(columns));
            _columns = columns;
        }

        private IColumns _columns;
        public IColumns Columns
        {
            get { return _columns; }
        }
    }
}
