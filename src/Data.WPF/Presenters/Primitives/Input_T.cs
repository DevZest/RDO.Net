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
        private Func<T, FlushError> _flushValidator;

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

        internal void SetFlushValidator(Func<T, FlushError> flushValidator)
        {
            if (flushValidator == null)
                throw new ArgumentNullException(nameof(flushValidator));

            _flushValidator = flushValidator;
        }

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
        }

        public abstract FlushErrorMessage GetFlushError(UIElement element);

        internal abstract void SetFlushError(UIElement element, FlushErrorMessage inputError);

        internal void ValidateFlush(T element)
        {
            if (_flushValidator == null)
                return;
            var oldflushError = GetFlushError(element);
            var flushError = _flushValidator(element);
            if (IsInputErrorChanged(flushError, oldflushError))
                SetFlushError(element, flushError.IsEmpty ? null : new FlushErrorMessage(flushError, element));
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

            ValidateFlush(element);
            if (GetFlushError(element) == null)
                FlushCore(element);
        }

        internal abstract void FlushCore(T element);
    }
}
