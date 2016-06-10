using DevZest.Data.Utilities;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
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

        public CaseOnExpression<TWhen, TResult> Then<TResult>(Column<TResult> then)
        {
            Check.NotNull(then, nameof(then));
            return new CaseOnExpression<TWhen, TResult>(_on).WhenThen(_when, then);
        }
    }

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

        public CaseOnExpression<TWhen, TResult> Then(Column<TResult> then)
        {
            Check.NotNull(then, nameof(then));
            return _caseOn.WhenThen(_when, then);
        }
    }
}
