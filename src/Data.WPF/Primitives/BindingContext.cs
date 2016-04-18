using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class BindingContext
    {
        internal static readonly BindingContext Current = new BindingContext();

        private Stack<Template> _templates = new Stack<Template>();
        private Stack<IBlockPresenter> _blockPresenters = new Stack<IBlockPresenter>();
        private Stack<RowPresenter> _rowPresenters = new Stack<RowPresenter>();

        private BindingContext()
        {
        }

        internal void Enter(RowPresenter row)
        {
            _templates.Push(row.Template);
            _blockPresenters.Push(null);
            _rowPresenters.Push(row);
        }

        internal void Enter(TemplateItem templateItem, UIElement uiElement)
        {
            _templates.Push(templateItem.Template);
            _blockPresenters.Push(uiElement.GetBlockPresenter());
            _rowPresenters.Push(uiElement.GetRowPresenter());
        }

        internal void Exit()
        {
            _templates.Pop();
            _blockPresenters.Pop();
            _rowPresenters.Pop();
        }

        private Template Template
        {
            get { return _templates.Count == 0 ? null : _templates.Peek(); }
        }

        internal RowManager RowManager
        {
            get { return Template == null ? null : Template.RowManager; }
        }

        public DataPresenter DataPresenter
        {
            get { return Template == null ? null : Template.DataPresenter; }
        }

        public IBlockPresenter BlockPresenter
        {
            get { return _blockPresenters.Count == 0 ? null : _blockPresenters.Peek(); }
        }

        public RowPresenter RowPresenter
        {
            get { return _rowPresenters.Count == 0 ? null : _rowPresenters.Peek(); }
        }
    }
}
