using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class BindingSource
    {
        internal static readonly BindingSource Current = new BindingSource();

        private Stack<Binding> _bindings = new Stack<Binding>();
        private Stack<UIElement> _elements = new Stack<UIElement>();

        internal void Enter(Binding binding, UIElement element)
        {
            _bindings.Push(binding);
            _elements.Push(element);
        }

        internal void Exit()
        {
            _bindings.Pop();
            _elements.Pop();
        }

        public DataView DataView
        {
            get
            {
                var binding = _bindings.Peek();
                return binding == null ? null : binding.Owner.Owner.Owner;
            }
        }

        public RowView RowView
        {
            get
            {
                var element = _elements.Peek();
                return element == null ? null : element.GetRowView();
            }
        }
    }
}
