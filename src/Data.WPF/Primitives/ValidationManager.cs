using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        private sealed class ValidationMessageCollection : ReadOnlyCollection<ValidationMessage>
        {
            public ValidationMessageCollection()
                : base(new List<ValidationMessage>())
            {
            }

            public void Add(ValidationMessage validationMessage)
            {
                base.Items.Add(validationMessage);
            }
        }

        private sealed class Validity : INotifyDataErrorInfo
        {
            private static DataErrorsChangedEventArgs SingletonEventArgs = new DataErrorsChangedEventArgs(string.Empty);

            public Validity(IReadOnlyList<ValidationMessage> validationMessages)
            {
                Debug.Assert(validationMessages != null);
                _validationMessages = validationMessages;
            }

            private IReadOnlyList<ValidationMessage> _validationMessages;
            public IReadOnlyList<ValidationMessage> ValidationMessages
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

        private Dictionary<Input, ValidationMessage> _flushingErrors = new Dictionary<Input, ValidationMessage>();
        internal bool UpdateFlushingError(Input input, ReverseBindingError error)
        {
            bool containsInput = _flushingErrors.ContainsKey(input);
            bool hasError = !error.IsEmpty;

            if (containsInput != hasError)
            {
                if (hasError)
                    _flushingErrors.Add(input, NewValidationMessage(error));
                else
                    _flushingErrors.Remove(input);
                return true;
            }
            else if (hasError)
            {
                var validationMessage = _flushingErrors[input];
                if (validationMessage.Id != error.MessageId || validationMessage.Description != error.Message)
                {
                    _flushingErrors[input] = NewValidationMessage(error);
                    return true;
                }
            }

            return false;
        }

        private ValidationMessage NewValidationMessage(ReverseBindingError error)
        {
            Debug.Assert(!error.IsEmpty);
            return new ValidationMessage(error.MessageId, ValidationSeverity.Error, ColumnSet.Empty, error.Message);
        }

        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationMessages = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _mergedValidationMessages = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();

        internal IReadOnlyList<ValidationMessage> GetErrors(Input input)
        {
            ValidationMessageCollection result = GetFlushingErrors(null, input);

            if (input.Binding is RowBinding && CurrentRow != null)
            {
                var currentRowValidationMessages = GetCurrentRowValidationMessages(_validationMessages);
                var currentRowMergedValidationMessages = GetCurrentRowValidationMessages(_mergedValidationMessages);
                result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Error, input);
                result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Error, input);
                if (Template.ValiditySeverity == ValidationSeverity.Warning)
                {
                    result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Warning, input);
                    result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Warning, input);
                }
            }

            if (result == null)
                return Array<ValidationMessage>.Empty;
            else
                return result;
        }

        private ValidationMessageCollection GetFlushingErrors(ValidationMessageCollection result, Input input)
        {
            ValidationMessage flushingError;
            if (_flushingErrors.TryGetValue(input, out flushingError))
                result = AddValidationMessage(result, flushingError);
            return result;
        }

        private IReadOnlyList<ValidationMessage> GetCurrentRowValidationMessages(Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> validationMessages)
        {
            if (CurrentRow != null)
                return Array<ValidationMessage>.Empty;
            IReadOnlyList<ValidationMessage> result;
            if (validationMessages.TryGetValue(CurrentRow, out result))
                return result;
            else
                return Array<ValidationMessage>.Empty;
        }

        private ValidationMessageCollection GetValidationMessages(ValidationMessageCollection result, IReadOnlyList<ValidationMessage> validationMessages, ValidationSeverity severity, Input input)
        {
            foreach (var validationMessage in validationMessages)
            {
                if (IsVisible(validationMessage, severity, input))
                    result = AddValidationMessage(result, validationMessage);
            }
            return result;
        }

        private static bool IsVisible(ValidationMessage validationMessage, ValidationSeverity severity, Input input)
        {
            if (validationMessage.Severity != severity)
                return false;

            foreach (var inputColumn in input.Columns)
            {
                if (validationMessage.Columns.Contains(inputColumn))
                    return true;
            }

            return false;
        }

        private static ValidationMessageCollection AddValidationMessage(ValidationMessageCollection collection, ValidationMessage validationMessage)
        {
            if (collection == null)
                collection = new ValidationMessageCollection();
            collection.Add(validationMessage);
            return collection;
        }


        public bool IsValidated { get; private set; }

        public bool IsValid(bool ignoreMergedResult = true)
        {
            if (_flushingErrors.Count > 0)
                return false;

            throw new NotImplementedException();
        }

        public bool Validate(bool ignoreMergedResult = true)
        {
            IsValidated = true;
            throw new NotImplementedException();
        }
    }
}
