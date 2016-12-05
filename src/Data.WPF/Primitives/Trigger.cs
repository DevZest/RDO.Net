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

        public RowInput<T> Bind<TData>(Column<TData> column, Func<T, TData> getValue)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            var result = RowInput<T>.Create(this, column, getValue);
            Initialize(result.Flush);
            return result;
        }

        public ScalarInput<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));
            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            var result = ScalarInput<T>.Create(this, scalar, getValue);
            Initialize(result.Flush);
            return result;
        }
    }
}
