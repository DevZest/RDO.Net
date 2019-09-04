namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a CASE ON expression column.
    /// </summary>
    /// <typeparam name="T">Data type of the column</typeparam>
    public struct CaseOn<T>
    {
        internal CaseOn(Column<T> on)
        {
            _on = on;
        }

        private readonly Column<T> _on;

        /// <summary>
        /// Constructs CASE ON...WHEN expression.
        /// </summary>
        /// <param name="when">The condition.</param>
        /// <returns>The result.</returns>
        public CaseOnWhen<T> When(Column<T> when)
        {
            when.VerifyNotNull(nameof(when));
            return new CaseOnWhen<T>(_on, when);
        }
    }
}
