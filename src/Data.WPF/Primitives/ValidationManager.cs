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

        private Dictionary<ScalarReverseBinding, ScalarValidationMessage> _scalarFlushingMessages = new Dictionary<ScalarReverseBinding, ScalarValidationMessage>();
        private Dictionary<RowReverseBinding, ValidationMessage> _currentRowFlushiningMessages = new Dictionary<RowReverseBinding, ValidationMessage>();
        internal bool UpdateFlushingMessage(ScalarReverseBinding reverseBinding, ReverseBindingMessage message)
        {
            return UpdateFlushingMessage(_scalarFlushingMessages, reverseBinding, message, ShouldCreateMessage, NewScalarValidationMessage);
        }

        private static bool ShouldCreateMessage(ScalarValidationMessage validationMessage, ReverseBindingMessage message)
        {
            return validationMessage.Id != message.MessageId || validationMessage.Severity != message.Severity || validationMessage.Description != message.Message;
        }

        private ScalarValidationMessage NewScalarValidationMessage(ReverseBindingMessage message)
        {
            Debug.Assert(!message.IsEmpty);
            return new ScalarValidationMessage(message.MessageId, message.Severity, ScalarSet.Empty, message.Message);
        }

        internal bool UpdateFlushingMessage(RowReverseBinding reverseBinding, ReverseBindingMessage message)
        {
            return UpdateFlushingMessage(_currentRowFlushiningMessages, reverseBinding, message, ShouldCreateMessage, NewValidationMessage);
        }

        private static bool ShouldCreateMessage(ValidationMessage validationMessage, ReverseBindingMessage message)
        {
            return validationMessage.Id != message.MessageId || validationMessage.Severity != message.Severity || validationMessage.Description != message.Message;
        }

        private ValidationMessage NewValidationMessage(ReverseBindingMessage message)
        {
            Debug.Assert(!message.IsEmpty);
            return new ValidationMessage(message.MessageId, message.Severity, ColumnSet.Empty, message.Message);
        }

        private static bool UpdateFlushingMessage<TReverseBinding, TMessage>(Dictionary<TReverseBinding, TMessage> flushingMessages,
            TReverseBinding reverseBinding, ReverseBindingMessage message, Func<TMessage, ReverseBindingMessage, bool> shouldCreateMessage,
            Func<ReverseBindingMessage, TMessage> createMessage)
        {
            bool containsReverseBinding = flushingMessages.ContainsKey(reverseBinding);
            bool notEmpty = !message.IsEmpty;

            if (containsReverseBinding != notEmpty)
            {
                if (notEmpty)
                    flushingMessages.Add(reverseBinding, createMessage(message));
                else
                    flushingMessages.Remove(reverseBinding);
                return true;
            }
            else if (notEmpty)
            {
                var validationMessage = flushingMessages[reverseBinding];
                if (shouldCreateMessage(validationMessage, message))
                {
                    flushingMessages[reverseBinding] = createMessage(message);
                    return true;
                }
            }

            return false;
        }

        private List<ScalarValidationMessage> _scalarValidationMessages = new List<ScalarValidationMessage>();

        internal IReadOnlyList<ScalarValidationMessage> GetMessages(ScalarReverseBinding reverseBinding)
        {
            var result = GetReverseBindingMessage(_scalarFlushingMessages, reverseBinding, ValidationSeverity.Error, Predict);
            result = GetValidationMessages(result, _scalarValidationMessages, ValidationSeverity.Error, reverseBinding, Predict);
            if (Template.ValiditySeverity == ValidationSeverity.Warning)
                result = GetValidationMessages(result, _scalarValidationMessages, ValidationSeverity.Warning, reverseBinding, Predict);
            return result;
        }

        private static bool Predict(ScalarValidationMessage validationMessage, ValidationSeverity severity)
        {
            return validationMessage.Severity == severity;
        }

        private static bool Predict(ScalarValidationMessage validationMessage, ValidationSeverity severity, ScalarReverseBinding reverseBinding)
        {
            if (!Predict(validationMessage, severity))
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
            var result = GetReverseBindingMessage(_currentRowFlushiningMessages, reverseBinding, ValidationSeverity.Error, Predict);

            if (reverseBinding.Binding is RowBinding && CurrentRow != null)
            {
                var currentRowValidationMessages = GetCurrentRowValidationMessages(_validationMessages);
                var currentRowMergedValidationMessages = GetCurrentRowValidationMessages(_mergedValidationMessages);
                result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Error, reverseBinding, Predict);
                result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Error, reverseBinding, Predict);
                if (Template.ValiditySeverity == ValidationSeverity.Warning)
                {
                    result = GetValidationMessages(result, currentRowValidationMessages, ValidationSeverity.Warning, reverseBinding, Predict);
                    result = GetValidationMessages(result, currentRowMergedValidationMessages, ValidationSeverity.Warning, reverseBinding, Predict);
                }
            }

            if (result == null)
                return Array<ValidationMessage>.Empty;
            else
                return result;
        }

        private List<TMessage> GetReverseBindingMessage<TReverseBinding, TMessage>(Dictionary<TReverseBinding, TMessage> reverseBindingMessages,
            TReverseBinding reverseBinding, ValidationSeverity severity, Func<TMessage, ValidationSeverity, bool> predict)
            where TReverseBinding : ReverseBinding
        {
            List<TMessage> result = null;
            TMessage message;
            if (reverseBindingMessages.TryGetValue(reverseBinding, out message))
            {
                if (predict(message, severity))
                    result = AddListItem(result, message);
            }
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

        private static bool Predict(ValidationMessage validationMessage, ValidationSeverity severity)
        {
            return validationMessage.Severity == severity;
        }

        private static bool Predict(ValidationMessage validationMessage, ValidationSeverity severity, RowReverseBinding reverseBinding)
        {
            if (!Predict(validationMessage, severity))
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
            if (_currentRowFlushiningMessages.Count > 0)
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
