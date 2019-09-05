namespace DevZest.Data
{
    /// <summary>
    /// Event data for <see cref="Model.ValueChanged"/> event.
    /// </summary>
    public class ValueChangedEventArgs : DataRowEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ValueChangedEventArgs"/>.
        /// </summary>
        /// <param name="dataRow">The DataRow.</param>
        /// <param name="columns">The columns.</param>
        public ValueChangedEventArgs(DataRow dataRow, IColumns columns)
            : base(dataRow)
        {
            columns.VerifyNotNull(nameof(columns));
            _columns = columns;
        }

        private IColumns _columns;
        /// <summary>
        /// Gets the columns.
        /// </summary>
        public IColumns Columns
        {
            get { return _columns; }
        }
    }
}
