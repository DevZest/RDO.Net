namespace DevZest.Data.Primitives
{
    public struct CaseOn<T>
    {
        internal CaseOn(Column<T> on)
        {
            _on = on;
        }

        private readonly Column<T> _on;

        public CaseOnExpression<T, TResult> WhenThen<TResult>(Column<T> when, Column<TResult> then)
        {
            return new CaseOnExpression<T, TResult>(_on).WhenThen(when, then);
        }
    }
}
