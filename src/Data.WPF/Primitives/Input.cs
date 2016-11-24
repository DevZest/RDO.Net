using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Input : INotifyDataErrorInfo
    {
        private static readonly DataErrorsChangedEventArgs SingletonDataErrorsChangedEventArgs = new DataErrorsChangedEventArgs(string.Empty);

        protected Input()
        {
        }

        public Binding Binding { get; private set; }

        internal void Seal(Binding binding)
        {
            Debug.Assert(binding != null);
            VerifyNotSealed();
            Binding = binding;
        }

        internal void VerifyNotSealed()
        {
            if (Binding != null)
                throw new InvalidOperationException(Strings.Input_VerifyNotSealed);
        }

        internal ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        internal abstract IColumnSet Columns { get; }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }

        public Template Template
        {
            get { return Binding.Template; }
        }

        private IReadOnlyList<ValidationMessage> _errors = Array<ValidationMessage>.Empty;
        private IReadOnlyList<ValidationMessage> Errors
        {
            get
            {
                if (_errors == null)
                    _errors = ValidationManager.GetErrors(this);
                return _errors;
            }
        }
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        internal void OnErrorsChanged()
        {
            _errors = null;
            var errorsChanged = ErrorsChanged;
            if (errorsChanged != null)
                errorsChanged(this, SingletonDataErrorsChangedEventArgs);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            /// Workaround: when using with <see cref="ValidationListener"/>, <param name="propertyName"/> will be passed in twice,
            /// as null and <see cref="string.Empty"/> respectively.
            /// We need to ignore one of them, otherwise duplicated results will be returned.
            return propertyName == null ? null : Errors;
        }
    }

    public abstract class Input<T> : Input
        where T : UIElement, new()
    {
        protected Input()
        {
        }

        private ReverseBinding<T> _reverseBinding;
        private TReverseBinding SetReverseBinding<TReverseBinding>(TReverseBinding value)
            where TReverseBinding : ReverseBinding<T>
        {
            _reverseBinding = value;
            return value;
        }

        internal sealed override IColumnSet Columns
        {
            get { return _reverseBinding == null ? null : _reverseBinding.Columns; }
        }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void Flush(T element)
        {
            Debug.Assert(_reverseBinding != null);
            _reverseBinding.Verify(element);
            var error = _reverseBinding.GetError(element);
            bool flushingErrorChanged = ValidationManager.UpdateFlushingError(this, error);
            _reverseBinding.Flush(element);
            if (flushingErrorChanged)
                OnErrorsChanged();
        }

        protected internal virtual bool ShouldRefresh(T element)
        {
            return false;
        }

        public ReverseRowBinding<T> Bind<TData>(Column<TData> column, Func<T, TData> dataGetter)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            VerifyNotSealed();
            return SetReverseBinding(ReverseRowBinding<T>.Create(this, column, dataGetter));
        }

        public ReverseScalarBinding<T> Bind(Action<T> flushAction)
        {
            if (flushAction == null)
                throw new ArgumentNullException(nameof(flushAction));
            VerifyNotSealed();
            return SetReverseBinding(new ReverseScalarBinding<T>(this, flushAction));
        }
    }
}
