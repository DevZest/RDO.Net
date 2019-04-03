using System;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class Trigger<T>
        where T : UIElement, new()
    {
        protected Trigger()
        {
        }

        protected internal Action<T> ExecuteAction { get; set; }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected void Execute(T element)
        {
            if (ExecuteAction != null)
                ExecuteAction(element);
        }

        public Trigger<T> WithExecuteAction(Action<T> executeAction)
        {
            ExecuteAction = executeAction;
            return this;
        }
    }
}
