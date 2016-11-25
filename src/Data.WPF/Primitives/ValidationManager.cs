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

        private Dictionary<ScalarReverseBinding, ScalarValidationMessage> _scalarFlushingErrors = new Dictionary<ScalarReverseBinding, ScalarValidationMessage>();
        private Dictionary<RowReverseBinding, ValidationMessage> _currentRowflushingErrors = new Dictionary<RowReverseBinding, ValidationMessage>();
        internal bool UpdateFlushingError(ScalarReverseBinding reverseBinding, ReverseBindingError error)
        {
            return UpdateFlushingError(_scalarFlushingErrors, reverseBinding, error, ShouldCreateMessage, NewScalarValidationMessage);
        }

        private static bool ShouldCreateMessage(ScalarValidationMessage message, ReverseBindingError error)
        {
            return message.Id != error.MessageId || message.Description != error.Message;
        }

        private ScalarValidationMessage NewScalarValidationMessage(ReverseBindingError error)
        {
            Debug.Assert(!error.IsEmpty);
            return new ScalarValidationMessage(error.MessageId, ValidationSeverity.Error, ScalarSet.Empty, error.Message);
        }

        internal bool UpdateFlushingError(RowReverseBinding reverseBinding, ReverseBindingError error)
        {
            return UpdateFlushingError(_currentRowflushingErrors, reverseBinding, error, ShouldCreateMessage, NewValidationMessage);
        }

        private static bool ShouldCreateMessage(ValidationMessage message, ReverseBindingError error)
        {
            return message.Id != error.MessageId || message.Description != error.Message;
        }

        private ValidationMessage NewValidationMessage(ReverseBindingError error)
        {
            Debug.Assert(!error.IsEmpty);
            return new ValidationMessage(error.MessageId, ValidationSeverity.Error, ColumnSet.Empty, error.Message);
        }

        private static bool UpdateFlushingError<TReverseBinding, TMessage>(Dictionary<TReverseBinding, TMessage> flushingErrors,
            TReverseBinding reverseBinding, ReverseBindingError error, Func<TMessage, ReverseBindingError, bool> shouldCreateMessage,
            Func<ReverseBindingError, TMessage> createMessage)
        {
            bool containsReverseBinding = flushingErrors.ContainsKey(reverseBinding);
            bool hasError = !error.IsEmpty;

            if (containsReverseBinding != hasError)
            {
                if (hasError)
                    flushingErrors.Add(reverseBinding, createMessage(error));
                else
                    flushingErrors.Remove(reverseBinding);
                return true;
            }
            else if (hasError)
            {
                var message = flushingErrors[reverseBinding];
                if (shouldCreateMessage(message, error))
                {
                    flushingErrors[reverseBinding] = createMessage(error);
                    return true;
                }
            }

            return false;
        }

        private List<ScalarValidationMessage> _scalarValidationMessages = new List<ScalarValidationMessage>();

        internal IReadOnlyList<ScalarValidationMessage> GetErrors(ScalarReverseBinding reverseBinding)
        {
            var result = GetFlushingErrors(_scalarFlushingErrors, reverseBinding);
            result = GetValidationMessages(result, _scalarValidationMessages, ValidationSeverity.Error, reverseBinding, IsVisible);
            if (Template.ValiditySeverity == ValidationSeverity.Warning)
                result = GetValidationMessages(result, _scalarValidationMessages, ValidationSeverity.Warning, reverseBinding, IsVisible);
            return result;
        }

        private static bool IsVisible(ScalarValidationMessage validationMessage, ValidationSeverity severity, ScalarReverseBinding reverseBinding)
        {
            if (validationMessage.Severity != severity)
                return false;

            foreach (var scalar in reverseBinding.Scalars)
            {
                if (validationMessage.Scalars.Contains(scalar))
                    return true;
            }

            return false;
        }

        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _validationMessages = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();
        private Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>> _mergedValidationMessages = new Dictionary<RowPresenter, IReadOnlyList<ValidationMessage>>();

        internal IReadOnlyList<ValidationMessage> GetErrors(RowReverseBinding reverseBinding)
        {
            var result = GetFlushingErrors(_currentRowflushingErrors, reverseBinding);

            if (reverseBinding.Binding is RowBinding && CurrentRow != null)
            {
                var currentRowValidationMessages = GetCurrentRowValidationMessages(_validationMessages);
                var currentRowMergedValidationMessages = GetCurrentRowValidationMessages(_mergedValidationMessages);
                result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Error, reverseBinding, IsVisible);
                result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Error, reverseBinding, IsVisible);
                if (Template.ValiditySeverity == ValidationSeverity.Warning)
                {
                    result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Warning, reverseBinding, IsVisible);
                    result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Warning, reverseBinding, IsVisible);
                }
            }

            if (result == null)
                return Array<ValidationMessage>.Empty;
            else
                return result;
        }

        private List<TMessage> GetFlushingErrors<TReverseBinding, TMessage>(Dictionary<TReverseBinding, TMessage> flushingErrors, TReverseBinding reverseBinding)
            where TReverseBinding : ReverseBinding
        {
            List<TMessage> result = null;
            TMessage flushingError;
            if (flushingErrors.TryGetValue(reverseBinding, out flushingError))
                result = AddListItem(result, flushingError);
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

        private List<TMessage> GetValidationMessages<TReverseBinding, TMessage>(List<TMessage> result, IReadOnlyList<TMessage> messages,
            ValidationSeverity severity, TReverseBinding reverseBinding, Func<TMessage, ValidationSeverity, TReverseBinding, bool> isVisible)
        {
            foreach (var validationMessage in messages)
            {
                if (isVisible(validationMessage, severity, reverseBinding))
                    result = AddListItem(result, validationMessage);
            }
            return result;
        }

        private static bool IsVisible(ValidationMessage validationMessage, ValidationSeverity severity, RowReverseBinding reverseBinding)
        {
            if (validationMessage.Severity != severity)
                return false;

            foreach (var column in reverseBinding.Columns)
            {
                if (validationMessage.Columns.Contains(column))
                    return true;
            }

            return false;
        }

        private static List<T> AddListItem<T>(List<T> list, T item)
        {
            if (list == null)
                list = new List<T>();
            list.Add(item);
            return list;
        }


        public bool IsValidated { get; private set; }

        public bool IsValid(bool ignoreMergedResult = true)
        {
            if (_currentRowflushingErrors.Count > 0)
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
