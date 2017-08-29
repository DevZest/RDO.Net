using System;
using DevZest.Data.Presenters.Primitives;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarPresenter : ElementPresenter
    {
        internal ScalarPresenter(Template template)
        {
            _template = template;
        }

        private readonly Template _template;
        public sealed override Template Template
        {
            get { return _template; }
        }

        public int FlowIndex { get; private set; } = -1;

        public FlushErrorMessage InputError { get; private set; }

        public FlushErrorMessage ValueError { get; private set; }

        internal void SetFlowIndex(int flowIndex)
        {
            FlowIndex = flowIndex;
            InputError = null;
            ValueError = null;
        }

        internal void SetErrors(FlushErrorMessage inputError, FlushErrorMessage valueError)
        {
            InputError = inputError;
            ValueError = valueError;
        }
    }
}
