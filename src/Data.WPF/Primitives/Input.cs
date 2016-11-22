using DevZest.Data.Primitives;
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

        public Binding Binding { get; internal set; }

        private ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        private Input FlushingInput
        {
            get { return ValidationManager.FlushingInput; }
            set { ValidationManager.FlushingInput = value; }
        }

        protected bool IsFlushing
        {
            get { return FlushingInput == this; }
        }

        internal void ExecFlush<T>(T element, Func<T, ValidationMessage> flushFunc)
        {
            FlushingInput = this;
            Columns = ColumnSet.Empty;
            ValidationMessage validationMessage;
            try
            {
                validationMessage = flushFunc(element);
            }
            finally
            {
                FlushingInput = null;
            }
        }

        internal IColumnSet Columns { get; private set; }

        internal void MergeColumn(Column column)
        {
            Columns = Columns.Merge(column);
        }

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

        private Func<T, ValidationMessage> _flushFunc;

        internal void Initialize(Binding binding, Func<T, ValidationMessage> flushFunc)
        {
            Debug.Assert(Binding == null && _flushFunc == null && binding != null && flushFunc != null);
            Binding = binding;
            _flushFunc = flushFunc;
        }

        internal void Initialize(Binding binding, Func<RowPresenter, T, ValidationMessage> flushFunc)
        {
            Initialize(binding, x => flushFunc(x.GetRowPresenter(), x));
        }

        protected internal abstract void Attach(T element);

        protected internal abstract void Detach(T element);

        protected internal void Flush(T element)
        {
            ExecFlush(element, _flushFunc);
        }

        protected internal virtual bool ShouldRefresh(T element)
        {
            return false;
        }
    }
}
