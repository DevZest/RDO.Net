namespace DevZest.Data.Primitives
{
    public struct CaseOnExpressionBuilder<T>
    {
        internal CaseOnExpressionBuilder(Column<T> on)
        {
            On = on;
        }

        public readonly Column<T> On;

        public CaseOnExpression<T, TResult> WhenThen<TResult>(Column<T> when, Column<TResult> then)
        {
            return new CaseOnExpression<T, TResult>(On).WhenThen(when, then);
        }
    }
}
