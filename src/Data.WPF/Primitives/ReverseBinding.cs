using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ReverseBinding<T>
        where T : UIElement, new()
    {
        internal ReverseBinding(Input<T> input)
        {
            Debug.Assert(input != null);
            Input = input;
        }

        public Input<T> Input { get; private set; }

        internal Func<T, ReverseBindingError> _onGetError;

        internal ReverseBindingError GetError(T element)
        {
            return _onGetError == null ? ReverseBindingError.Empty : _onGetError(element);
        }

        internal abstract IColumnSet Columns { get; }

        internal abstract void Flush(T element);

        private bool IsSealed
        {
            get { return Input.Binding != null; }
        }

        internal void VerifyNotSealed()
        {
            if (IsSealed)
                throw new InvalidOperationException(Strings.ReverseBinding_VerifyNotSealed);
        }
    }
}
