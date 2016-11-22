using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
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

        private ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        internal IColumnSet Columns { get; private set; }

        public bool HasErrors
        {
            get { return ValidationManager.HasErrors(this); }
        }

        public Template Template
        {
            get { return Binding.Template; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            /// When using with <see cref="ValidationListener"/>, <param name="propertyName"/> will be passed in twice,
            /// as null and <see cref="string.Empty"/> respectively.
            /// We need to ignore one of them, otherwise duplicated results will be returned.
            return propertyName == null ? null : ValidationManager.GetErrors(this);
        }

        internal void RefreshValidation()
        {
            var errorsChanged = ErrorsChanged;
            if (errorsChanged != null)
                errorsChanged(this, SingletonDataErrorsChangedEventArgs);
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

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void Flush(T element)
        {
            Debug.Assert(_reverseBinding != null);
            _reverseBinding.Flush(element);
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
