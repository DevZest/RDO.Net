using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public class ValueChangedEventArgs : DataRowEventArgs
    {
        public ValueChangedEventArgs(DataRow dataRow, IColumns columns)
            : base(dataRow)
        {
            Check.NotNull(columns, nameof(columns));
            _columns = columns;
        }

        private IColumns _columns;
        public IColumns Columns
        {
            get { return _columns; }
        }
    }
}
