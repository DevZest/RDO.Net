using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public class ValueChangedEventArgs : DataRowEventArgs
    {
        public ValueChangedEventArgs(DataRow dataRow, Column column)
            : base(dataRow)
        {
            Check.NotNull(column, nameof(column));
            _column = column;
        }

        private Column _column;
        public Column Column
        {
            get { return _column; }
        }
    }
}
