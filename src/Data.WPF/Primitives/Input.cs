using DevZest.Data.Windows.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Input<T> : INotifyDataErrorInfo
        where T : UIElement, new()
    {
        #region INotifyDataErrorInfo

        private IList<ValidationMessage> _dataErrorInfo = Array<ValidationMessage>.Empty;
        event EventHandler<DataErrorsChangedEventArgs> _dataErrorInfoChanged;

        bool INotifyDataErrorInfo.HasErrors
        {
            get { return _dataErrorInfo.Count > 0; }
        }

        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { _dataErrorInfoChanged += value; }
            remove { _dataErrorInfoChanged -= value; }
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            /// Workaround: when using with <see cref="ValidationListener"/>, <param name="propertyName"/> will be passed in twice,
            /// as null and <see cref="string.Empty"/> respectively.
            /// We need to ignore one of them, otherwise duplicated results will be returned.
            return propertyName == null ? null : _dataErrorInfo;
        }

        private void NotifyDataErrorInfoChanged()
        {
            var errorsChanged = _dataErrorInfoChanged;
            if (errorsChanged != null)
                errorsChanged(this, Singleton.DataErrorsChangedEventArgs);
        }

        #endregion

        internal Input(Trigger<T> flushTrigger)
        {
            _flushTrigger = flushTrigger;
        }

        private Trigger<T> _flushTrigger;
        private Trigger<T> _preValidtorTrigger;
        private Func<T, ValidationMessage> _preValidator;
        private ValidationMessage _preValidatorMessage;
        private ValidationMessage _asyncValidatorMessage;

        private int RefreshDataErrorInfo()
        {
            var wasEmpty = _dataErrorInfo.Count == 0;

            if (_dataErrorInfo != Array<ValidationMessage>.Empty)
                _dataErrorInfo.Clear();

            var result = AddDataErrorInfo(ValidationSeverity.Error);
            if (Template.ValiditySeverity == ValidationSeverity.Warning)
                AddDataErrorInfo(ValidationSeverity.Warning);

            if (_dataErrorInfo.Count == 0)
                _dataErrorInfo = Array<ValidationMessage>.Empty;

            if (!wasEmpty || _dataErrorInfo.Count > 0)
                NotifyDataErrorInfoChanged();

            return result;
        }

        private void AddDataErrorInfo(ValidationMessage message)
        {
            Debug.Assert(!message.IsEmpty);
            if (_dataErrorInfo == Array<ValidationMessage>.Empty)
                _dataErrorInfo = new List<ValidationMessage>();
            _dataErrorInfo.Add(message);
        }

        private int AddDataErrorInfo(ValidationSeverity severity)
        {

            if (_preValidatorMessage.IsSeverity(severity))
                AddDataErrorInfo(_preValidatorMessage);

            var result = 0;
            if (severity == ValidationSeverity.Error)
                result = AddValidationErrors();
            else
                AddValidationWarnings();
            if (_asyncValidatorMessage.IsSeverity(severity))
                AddDataErrorInfo(_asyncValidatorMessage);
            foreach (var message in GetMergedValidationMessages(severity))
                AddDataErrorInfo(message);

            return result;
        }

        private int AddValidationErrors()
        {
            return _preValidatorMessage.IsError ? 1 : AddValidationMessages(ValidationSeverity.Error);
        }

        private void AddValidationWarnings()
        {
            AddValidationMessages(ValidationSeverity.Warning);
        }

        private int AddValidationMessages(ValidationSeverity severity)
        {
            var oldCount = _dataErrorInfo.Count;
            foreach (var message in GetValidationMessages(severity))
                AddDataErrorInfo(message);
            return _dataErrorInfo.Count - oldCount;
        }

        internal abstract IEnumerable<ValidationMessage> GetValidationMessages(ValidationSeverity severity);

        internal abstract IEnumerable<ValidationMessage> GetMergedValidationMessages(ValidationSeverity severity);

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

        private Template Template
        {
            get { return Binding.Template; }
        }

        internal void SetPreValidator(Func<T, ValidationMessage> preValidator, Trigger<T> preValidatorTrigger)
        {
            _preValidator = preValidator;
            if (preValidatorTrigger != null)
            {
                preValidatorTrigger.Initialize(PreValidate);
                _preValidtorTrigger = preValidatorTrigger;
            }
        }

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
            if (_preValidtorTrigger != null)
                _preValidtorTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            if (_preValidtorTrigger != null)
                _preValidtorTrigger.Detach(element);
            _flushTrigger.Detach(element);
        }

        private void RefreshPreValidatorMessage(T element)
        {
            _preValidatorMessage = _preValidator == null ? ValidationMessage.Empty : _preValidator(element);
        }

        private void PreValidate(T element)
        {
            RefreshPreValidatorMessage(element);
            RefreshDataErrorInfo();
        }

        internal void Flush(T element)
        {
            RefreshPreValidatorMessage(element);

            bool isFlushed = false;
            if (!_preValidatorMessage.IsSeverity(ValidationSeverity.Error))
            {
                isFlushed = DoFlush(element);
                if (isFlushed)
                {
                    _asyncValidatorMessage = ValidationMessage.Empty;
                    RefreshValidationMessages();
                }
            }

            var errorCount = RefreshDataErrorInfo();
            if (isFlushed && errorCount == 0)
                PostValidate();
        }

        internal abstract bool DoFlush(T element);

        internal abstract void RefreshValidationMessages();

        private void PostValidate()
        {
            throw new NotImplementedException();
        }
    }
}
