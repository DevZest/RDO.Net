using DevZest.Data.Windows.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ReverseScalarBinding<T> : ReverseBinding<T>
        where T : UIElement, new()
    {
        internal ReverseScalarBinding(Input<T> input, Action<T> flushAction)
            : base(input)
        {
            Debug.Assert(flushAction != null);
            _flushAction = flushAction;
        }

        private readonly Action<T> _flushAction;

        public ReverseScalarBinding<T> OnGetError(Func<T, ReverseBindingError> onGetError)
        {
            VerifyNotSealed();
            base._onGetError = onGetError;
            return this;
        }

        internal override IColumnSet Columns
        {
            get { return null; }
        }

        internal override void Flush(T element)
        {
            _flushAction(element);
        }
    }
}
