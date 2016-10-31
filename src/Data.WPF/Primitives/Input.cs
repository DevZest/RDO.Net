using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Input
    {
        protected Input()
        {
        }

        public Binding Binding { get; internal set; }

        public Template Template
        {
            get { return Binding.Template; }
        }
    }

    public abstract class Input<T> : Input
        where T : UIElement, new()
    {
        protected Input()
        {
        }

        private Action<T> _flushAction;

        internal void Initialize(Binding binding, Action<T> flushAction)
        {
            Debug.Assert(Binding == null && _flushAction == null && binding != null && flushAction != null);
            Binding = binding;
            _flushAction = flushAction;
        }

        internal void Initialize(Binding binding, Action<RowPresenter, T> flushAction)
        {
            Initialize(binding, x => flushAction(x.GetRowPresenter(), x));
        }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void Flush(T element)
        {
            Template.FlushingInput = this;
            try
            {
                _flushAction(element);
            }
            finally
            {
                Template.FlushingInput = null;
            }
        }

        protected internal virtual bool ShouldRefresh(T element)
        {
            return false;
        }

        protected bool IsFlushing
        {
            get { return Template.FlushingInput == this; }
        }
    }
}
