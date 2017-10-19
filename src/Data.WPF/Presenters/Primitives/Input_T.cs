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
        private string _flushValidatorId;
        private Func<T, string> _flushValidator;

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

        internal void SetFlushValidator(Func<T, string> flushValidator, string flushValidatorId)
        {
            if (flushValidator == null)
                throw new ArgumentNullException(nameof(flushValidator));

            _flushValidator = flushValidator;
            _flushValidatorId = flushValidatorId;
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
            var flushErrorDescription = _flushValidator(element);
            if (IsFlushErrorChanged(_flushValidatorId, flushErrorDescription, oldflushError))
                SetFlushError(element, string.IsNullOrEmpty(flushErrorDescription) ? null : new FlushErrorMessage(_flushValidatorId, flushErrorDescription, element));
        }

        private static bool IsFlushErrorChanged(string flushErrorId, string flushErrorDescription, FlushErrorMessage flushErrorMessage)
        {
            return string.IsNullOrEmpty(flushErrorDescription) ? flushErrorMessage != null
                : flushErrorMessage == null || flushErrorMessage.Id != flushErrorId || flushErrorMessage.Description != flushErrorDescription;
        }

        public bool IsFlushing { get; private set; }

        internal void Flush(T element)
        {
            if (Binding.IsRefreshing)
                return;

            IsFlushing = true;
            ValidateFlush(element);
            if (GetFlushError(element) == null)
                FlushCore(element);
            IsFlushing = false;
        }

        internal abstract void FlushCore(T element);
    }
}
