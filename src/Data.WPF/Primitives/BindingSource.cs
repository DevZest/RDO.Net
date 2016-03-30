using DevZest.Data.Windows.Primitives;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class BindingSource
    {
        internal static readonly BindingSource Current = new BindingSource();

        private Stack<Template> _templates = new Stack<Template>();
        private Stack<RowPresenter> _rowPresenters = new Stack<RowPresenter>();

        private BindingSource()
        {
        }

        internal void Enter(TemplateItem templateItem, UIElement uiElement)
        {
            _templates.Push(templateItem.Template);
            _rowPresenters.Push(uiElement.GetRowPresenter());
        }

        internal void Exit()
        {
            _templates.Pop();
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

        internal ElementManager ElementManager
        {
            get { return Template == null ? null : Template.ElementManager; }
        }

        internal LayoutManager LayoutManager
        {
            get { return Template == null ? null : Template.LayoutManager; }
        }

        public DataPresenter DataPresenter
        {
            get { return Template == null ? null : Template.DataPresenter; }
        }

        public RowPresenter RowPresenter
        {
            get { return _rowPresenters.Count == 0 ? null : _rowPresenters.Peek(); }
        }
    }
}
