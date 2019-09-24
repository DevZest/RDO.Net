using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents scalar data binding context.
    /// </summary>
    public sealed class ScalarPresenter : ElementPresenter
    {
        internal ScalarPresenter(Template template)
        {
            _template = template;
        }

        private readonly Template _template;
        /// <inheritdoc/>
        public sealed override Template Template
        {
            get { return _template; }
        }

        /// <summary>
        /// Gets the flow index.
        /// </summary>
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
