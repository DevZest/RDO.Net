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

        public RowReverseBinding<T> Bind<TData>(Column<TData> column, Func<T, TData> dataGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            var result = RowReverseBinding<T>.Create(this, column, dataGetter);
            Initialize(result.Flush);
            return result;
        }

        public ScalarReverseBinding<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> dataGetter)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            var result = ScalarReverseBinding<T>.Create(this, scalar, dataGetter);
            Initialize(result.Flush);
            return result;
        }
    }
}
