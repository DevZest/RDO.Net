using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Trigger<T>
        where T : UIElement, new()
    {
        protected Trigger()
        {
        }

        private Action<T> _executeAction;
        internal void Initialize(Action<T> executeAction)
        {
            Debug.Assert(executeAction != null);
            if (_executeAction != null)
                throw new InvalidOperationException(Strings.Trigger_AlreadyInitialized);
            _executeAction = executeAction;
        }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected void Execute(T element)
        {
            _executeAction(element);
        }
    }
}
