using System;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;

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

        public FlushError InputError { get; private set; }

        public FlushError ValueError { get; private set; }

        private Stack<int> _flowIndexes = new Stack<int>();

        internal void EnterSetup(int flowIndex)
        {
            _flowIndexes.Push(FlowIndex);
            FlowIndex = flowIndex;
            InputError = null;
            ValueError = null;
        }

        internal void ExitSetup()
        {
            FlowIndex = _flowIndexes.Pop();
        }

        internal void SetErrors(FlushError inputError, FlushError valueError)
        {
            InputError = inputError;
            ValueError = valueError;
        }
    }
}
