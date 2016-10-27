using System;
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
        protected Input(Action<RowPresenter, T> flushAction)
        {
            if (flushAction == null)
                throw new ArgumentNullException(nameof(flushAction));
            _flushAction = flushAction;
        }

        private readonly Action<RowPresenter, T> _flushAction;

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void Flush(T element)
        {
            Template.FlushingInput = this;
            try
            {
                var rowPresenter = element.GetRowPresenter();
                _flushAction(rowPresenter, element);
            }
            finally
            {
                Template.FlushingInput = null;
            }
        }
    }
}
