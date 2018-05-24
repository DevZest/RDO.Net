using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public struct CaseOn<T>
    {
        internal CaseOn(Column<T> on)
        {
            _on = on;
        }

        private readonly Column<T> _on;

        public CaseOnWhen<T> When(Column<T> when)
        {
            Check.NotNull(when, nameof(when));
            return new CaseOnWhen<T>(_on, when);
        }
    }
}
