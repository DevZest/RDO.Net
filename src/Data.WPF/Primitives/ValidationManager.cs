using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        private sealed class Validity : INotifyDataErrorInfo
        {
            private static DataErrorsChangedEventArgs SingletonEventArgs = new DataErrorsChangedEventArgs(string.Empty);

            public Validity(IReadOnlyList<ValidationMessage<IColumnSet>> validationMessages)
            {
                Debug.Assert(validationMessages != null);
                _validationMessages = validationMessages;
            }

            private IReadOnlyList<ValidationMessage<IColumnSet>> _validationMessages;
            public IReadOnlyList<ValidationMessage<IColumnSet>> ValidationMessages
            {
                get { return _validationMessages; }
                set
                {
                    _validationMessages = value;
                    OnErrorsChanged();
                }
            }

            public bool HasErrors
            {
                get { return _validationMessages.Count > 0; }
            }

            public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

            private void OnErrorsChanged()
            {
                var errorsChanged = ErrorsChanged;
                if (errorsChanged != null)
                    errorsChanged(this, SingletonEventArgs);
            }

            public IEnumerable GetErrors(string propertyName)
            {
                return propertyName == null ? null : _validationMessages;
            }
        }

        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        internal void RefreshCurrentRowValidationMessages()
        {
            throw new NotImplementedException();
        }

        internal void RefreshScalarValidationMessages()
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<ValidationMessage> GetValidationMessages<T>(RowReverseBinding<T> reverseBinding, ValidationSeverity severity)
            where T : UIElement, new()
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<ValidationMessage> GetValidationMessages<T>(ScalarReverseBinding<T> reverseBinding, ValidationSeverity severity)
            where T : UIElement, new()
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<ValidationMessage> GetMergedValidationMessages<T>(RowReverseBinding<T> reverseBinding, ValidationSeverity severity)
            where T : UIElement, new()
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<ValidationMessage> GetMergedValidationMessages<T>(ScalarReverseBinding<T> reverseBinding, ValidationSeverity severity)
            where T : UIElement, new()
        {
            throw new NotImplementedException();
        }

        public bool IsValidated { get; private set; }

        public bool IsValid(bool ignoreMergedResult = true)
        {
            throw new NotImplementedException();
        }

        public bool Validate(bool ignoreMergedResult = true)
        {
            IsValidated = true;
            throw new NotImplementedException();
        }
    }
}
