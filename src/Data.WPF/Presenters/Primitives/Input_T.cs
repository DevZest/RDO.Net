using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class Input<T>
        where T : UIElement, new()
    {
        internal Input(Trigger<T> flushTrigger)
        {
            if (flushTrigger == null)
                throw new ArgumentNullException(nameof(flushTrigger));
            VerifyNotInitialized(flushTrigger, nameof(flushTrigger));
            _flushTrigger = flushTrigger;
            _flushTrigger.ExecuteAction = Flush;
        }

        private void VerifyNotInitialized(Trigger<T> trigger, string paramName)
        {
            if (trigger.ExecuteAction != null)
                throw new ArgumentException(Strings.Input_TriggerAlreadyInitialized, paramName);
        }

        private Trigger<T> _flushTrigger;
        private Trigger<T> _inputValidationTrigger;
        private Func<T, FlushError> _inputValidator;

        public abstract TwoWayBinding Binding { get; }

        internal void VerifyNotSealed()
        {
            Binding.VerifyNotSealed();
        }

        internal InputManager InputManager
        {
            get { return Template.InputManager; }
        }

        internal Template Template
        {
            get { return Binding.Template; }
        }

        internal void SetInputValidator(Func<T, FlushError> inputValidator, Trigger<T> inputValidationTrigger)
        {
            if (inputValidator == null)
                throw new ArgumentNullException(nameof(inputValidator));
            if (inputValidationTrigger != null)
                VerifyNotInitialized(inputValidationTrigger, nameof(inputValidationTrigger));

            _inputValidator = inputValidator;
            if (inputValidationTrigger != null)
            {
                _inputValidationTrigger = inputValidationTrigger;
                _inputValidationTrigger.ExecuteAction = ValidateInput;
            }
        }

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
            if (_inputValidationTrigger != null)
                _inputValidationTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            if (_inputValidationTrigger != null)
                _inputValidationTrigger.Detach(element);
            _flushTrigger.Detach(element);
        }

        public abstract FlushErrorMessage GetFlushError(UIElement element);

        internal abstract void SetFlushError(UIElement element, FlushErrorMessage inputError);

        internal void ValidateInput(T element)
        {
            if (_inputValidator == null)
                return;
            var oldInputError = GetFlushError(element);
            var inputError = _inputValidator(element);
            if (IsInputErrorChanged(inputError, oldInputError))
                SetFlushError(element, inputError.IsEmpty ? null : new FlushErrorMessage(inputError, element));
        }

        private static bool IsInputErrorChanged(FlushError inputError, FlushErrorMessage viewInputError)
        {
            return inputError.IsEmpty ? viewInputError != null
                : viewInputError == null || viewInputError.Id != inputError.Id || viewInputError.Description != inputError.Description;
        }

        internal void Flush(T element)
        {
            if (Binding.IsRefreshing)
                return;

            ValidateInput(element);
            if (GetFlushError(element) == null)
                FlushCore(element);
        }

        internal abstract void FlushCore(T element);
    }
}
