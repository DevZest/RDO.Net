namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents CASE WHEN... expression.
    /// </summary>
    public struct CaseWhen
    {
        internal CaseWhen(_Boolean when)
        {
            _when = when;
        }

        private readonly _Boolean _when;

        /// <summary>
        /// Constructs CASE WHEN...THEN... expression.
        /// </summary>
        /// <typeparam name="T">The data type of column for THEN expression.</typeparam>
        /// <param name="then">The column for THEN expression.</param>
        /// <returns>The result expression.</returns>
        public CaseExpression<T> Then<T>(Column<T> then)
        {
            then.VerifyNotNull(nameof(then));
            return new CaseExpression<T>().WhenThen(_when, then);
        }
    }

    /// <summary>
    /// Represents CASE...WHEN... expression.
    /// </summary>
    /// <typeparam name="T">Type data type of CASE... expression.</typeparam>
    public struct CaseWhen<T>
    {
        internal CaseWhen(CaseExpression<T> caseExpr, _Boolean when)
        {
            _case = caseExpr;
            _when = when;
        }

        private readonly CaseExpression<T> _case;
        private readonly _Boolean _when;

        /// <summary>
        /// Constructs CASE...WHEN...THEN... expression.
        /// </summary>
        /// <param name="then">The column for THEN... expression.</param>
        /// <returns>The result expression.</returns>
        public CaseExpression<T> Then(Column<T> then)
        {
            then.VerifyNotNull(nameof(then));
            _case.WhenThen(_when, then);
            return _case;
        }
    }
}
