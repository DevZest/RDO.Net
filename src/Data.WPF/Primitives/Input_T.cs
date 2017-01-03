using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class Input<T>
        where T : UIElement, new()
    {
        internal Input(Trigger<T> flushTrigger)
        {
            _flushTrigger = flushTrigger;
            _flushTrigger.Initialize(Flush);
        }

        private Trigger<T> _flushTrigger;
        private Trigger<T> _preValidtorTrigger;
        private Func<T, string> _preValidator;
        private string _preValidatorError;
        internal bool HasPreValidatorError
        {
            get { return !string.IsNullOrEmpty(_preValidatorError); }
        }
        internal ValidationMessage PreValidatorError
        {
            get
            {
                Debug.Assert(HasPreValidatorError);
                return ValidationMessage.Error(_preValidatorError);
            }
        }

        public abstract TwoWayBinding Binding { get; }

        internal void VerifyNotSealed()
        {
            if (Binding != null)
                throw new InvalidOperationException(Strings.ReverseBinding_VerifyNotSealed);
        }

        internal ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        internal Template Template
        {
            get { return Binding.Template; }
        }

        internal void SetPreValidator(Func<T, string> preValidator, Trigger<T> preValidatorTrigger)
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

        private string RefreshPreValidatorError(T element)
        {
            var oldValue = _preValidatorError;
            _preValidatorError = _preValidator == null ? string.Empty : _preValidator(element);
            return oldValue;
        }

        private void PreValidate(T element)
        {
            var oldValue = RefreshPreValidatorError(element);
            if (_preValidatorError != oldValue)
                OnPreValidatorErrorChanged();
        }

        private void OnPreValidatorErrorChanged()
        {
            ValidationManager.InvalidateView();
        }

        internal void Flush(T element)
        {
            var oldPreValidatorError = RefreshPreValidatorError(element);
            if (!string.IsNullOrEmpty(_preValidatorError))
            {
                if (oldPreValidatorError != _preValidatorError)
                    OnPreValidatorErrorChanged();
                return;
            }

            FlushCore(element);
        }

        internal abstract void FlushCore(T element);
    }
}
