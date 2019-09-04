using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a CASE ON...WHEN expression.
    /// </summary>
    /// <typeparam name="TWhen">Data type of WHEN column.</typeparam>
    public struct CaseOnWhen<TWhen>
    {
        internal CaseOnWhen(Column<TWhen> on, Column<TWhen> when)
        {
            Debug.Assert(on != null);
            Debug.Assert(when != null);
            _on = on;
            _when = when;
        }

        private readonly Column<TWhen> _on;
        private readonly Column<TWhen> _when;

        /// <summary>
        /// Constructs the CASE ON...WHEN...THEN expression.
        /// </summary>
        /// <typeparam name="TResult">Data type of THEN column.</typeparam>
        /// <param name="then">The column for THEN expression.</param>
        /// <returns>The result expression.</returns>
        public CaseOnExpression<TWhen, TResult> Then<TResult>(Column<TResult> then)
        {
            then.VerifyNotNull(nameof(then));
            return new CaseOnExpression<TWhen, TResult>(_on).WhenThen(_when, then);
        }
    }

    /// <summary>
    /// Represents a CASE ON...WHEN expression.
    /// </summary>
    /// <typeparam name="TWhen">Data type of WHEN column.</typeparam>
    /// <typeparam name="TResult">Data type of the result column.</typeparam>
    public struct CaseOnWhen<TWhen, TResult>
    {
        internal CaseOnWhen(CaseOnExpression<TWhen, TResult> caseOn, Column<TWhen> when)
        {
            Debug.Assert(caseOn != null);
            Debug.Assert(when != null);
            _caseOn = caseOn;
            _when = when;
        }

        private readonly CaseOnExpression<TWhen, TResult> _caseOn;
        private readonly Column<TWhen> _when;

        /// <summary>
        /// Constructs the CASE ON...WHEN...THEN expression.
        /// </summary>
        /// <param name="then">The column for THEN expression.</param>
        /// <returns>The result expression.</returns>
        public CaseOnExpression<TWhen, TResult> Then(Column<TResult> then)
        {
            then.VerifyNotNull(nameof(then));
            return _caseOn.WhenThen(_when, then);
        }
    }
}
