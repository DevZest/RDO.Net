using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public struct CaseWhen
    {
        internal CaseWhen(_Boolean when)
        {
            _when = when;
        }

        private readonly _Boolean _when;

        public CaseExpression<T> Then<T>(Column<T> then)
        {
            Check.NotNull(then, nameof(then));
            return new CaseExpression<T>().WhenThen(_when, then);
        }
    }

    public struct CaseWhen<T>
    {
        internal CaseWhen(CaseExpression<T> caseExpr, _Boolean when)
        {
            _case = caseExpr;
            _when = when;
        }

        private readonly CaseExpression<T> _case;
        private readonly _Boolean _when;

        public CaseExpression<T> Then(Column<T> then)
        {
            Check.NotNull(then, nameof(then));
            _case.WhenThen(_when, then);
            return _case;
        }
    }
}
