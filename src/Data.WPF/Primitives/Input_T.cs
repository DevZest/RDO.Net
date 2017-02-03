using DevZest.Data.Primitives;
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
        private Trigger<T> _inputValidationTrigger;
        private Func<T, InputError> _inputValidator;
        internal bool HasInputError
        {
            get { return InputError != null; }
        }
        internal ViewInputError InputError { get; private set; }

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

        internal void SetInputValidator(Func<T, InputError> inputValidator, Trigger<T> inputValidationTrigger)
        {
            _inputValidator = inputValidator;
            if (inputValidationTrigger != null)
            {
                inputValidationTrigger.Initialize(ValidateInput);
                _inputValidationTrigger = inputValidationTrigger;
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

        private void ValidateInput(T element)
        {
            var inputError = _inputValidator(element);
            if (IsInputErrorChanged(inputError, InputError))
            {
                InputError = inputError.IsEmpty ? null : new ViewInputError(inputError, element);
                OnInputErrorChanged();
            }
        }

        private static bool IsInputErrorChanged(InputError inputError, ViewInputError viewInputError)
        {
            return inputError.IsEmpty ? viewInputError != null
                : viewInputError == null || viewInputError.Id != inputError.Id || viewInputError.Description != inputError.Description;
        }

        private void OnInputErrorChanged()
        {
            ValidationManager.InvalidateElements();
        }

        internal void Flush(T element)
        {
            ValidateInput(element);
            if (!HasInputError)
                FlushCore(element);
        }

        internal abstract void FlushCore(T element);
    }
}
