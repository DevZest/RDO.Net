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

        private Stack<int> _flowIndexes = new Stack<int>();

        internal void EnterSetup(int flowIndex)
        {
            _flowIndexes.Push(FlowIndex);
            FlowIndex = flowIndex;
        }

        internal void ExitSetup()
        {
            FlowIndex = _flowIndexes.Pop();
        }
    }
}
