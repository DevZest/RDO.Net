using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ReverseBinding : INotifyDataErrorInfo
    {
        private static readonly DataErrorsChangedEventArgs SingletonDataErrorsChangedEventArgs = new DataErrorsChangedEventArgs(string.Empty);

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
                throw new InvalidOperationException(Strings.ReverseBinding_VerifyNotSealed);
        }

        internal ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }

        public Template Template
        {
            get { return Binding.Template; }
        }

        private IReadOnlyList<object> _errors = Array<ValidationMessage>.Empty;
        private IReadOnlyList<object> Errors
        {
            get
            {
                if (_errors == null)
                    _errors = GetErrors();
                return _errors;
            }
        }

        internal abstract IReadOnlyList<object> GetErrors();

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
}
