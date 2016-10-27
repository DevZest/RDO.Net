using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Input
    {
        protected Input()
        {
        }

        public RowBinding RowBinding { get; internal set; }

        public Template Template
        {
            get { return RowBinding.Template; }
        }
    }

    public abstract class Input<T> : Input
        where T : UIElement, new()
    {
        protected Input(Action<RowPresenter, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        private readonly Action<RowPresenter, T> _action;

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void ExecuteAction(T element)
        {
            Template.ExecutingInput = this;
            try
            {
                var rowPresenter = element.GetRowPresenter();
                _action(rowPresenter, element);
            }
            finally
            {
                Template.ExecutingInput = null;
            }
        }
    }
}
