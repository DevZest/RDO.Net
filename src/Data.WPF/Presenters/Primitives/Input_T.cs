using System;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class Input<T>
        where T : UIElement, new()
    {
        internal Input(Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
        {
            if (flushTrigger == null)
                throw new ArgumentNullException(nameof(flushTrigger));
            VerifyNotInitialized(flushTrigger, nameof(flushTrigger));
            _flushTrigger = flushTrigger;
            _flushTrigger.ExecuteAction = Flush;

            if (progressiveFlushTrigger != null)
            {
                _progressFlushTrigger = progressiveFlushTrigger;
                _progressFlushTrigger.ExecuteAction = ProgressiveFlush;
            }
        }

        private void VerifyNotInitialized(Trigger<T> trigger, string paramName)
        {
            if (trigger.ExecuteAction != null)
                throw new ArgumentException(DiagnosticMessages.Input_TriggerAlreadyInitialized, paramName);
        }

        private readonly Trigger<T> _flushTrigger;
        private readonly Trigger<T> _progressFlushTrigger;
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

        internal void SetFlushValidator(Func<T, string> flushValidator)
        {
            if (flushValidator == null)
                throw new ArgumentNullException(nameof(flushValidator));

            _flushValidator = flushValidator;
        }

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
            if (_progressFlushTrigger != null)
                _progressFlushTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
            if (_progressFlushTrigger != null)
                _progressFlushTrigger.Detach(element);
        }

        public abstract FlushError GetFlushError(UIElement element);

        internal abstract void SetFlushError(UIElement element, FlushError inputError);

        internal void ValidateFlush(T element)
        {
            if (_flushValidator == null)
                return;
            var oldflushError = GetFlushError(element);
            var flushErrorDescription = _flushValidator(element);
            if (IsFlushErrorChanged(flushErrorDescription, oldflushError))
                SetFlushError(element, string.IsNullOrEmpty(flushErrorDescription) ? null : new FlushError(flushErrorDescription, element));
        }

        private static bool IsFlushErrorChanged(string flushErrorMessage, FlushError flushError)
        {
            return string.IsNullOrEmpty(flushErrorMessage) ? flushError != null
                : flushError == null || flushError.Message != flushErrorMessage;
        }

        public bool IsFlushing { get; private set; }

        internal void Flush(T element)
        {
            PerformFlush(element, _progressFlushTrigger == null || IsValidationVisible);
        }

        internal abstract bool IsValidationVisible { get; }

        private void ProgressiveFlush(T element)
        {
            if (element.GetRowPresenter().IsEditing)
                PerformFlush(element, true);
        }

        private void PerformFlush(T element, bool makeProgress)
        {
            if (Binding.IsRefreshing)
                return;

            IsFlushing = true;
            ValidateFlush(element);
            if (GetFlushError(element) == null)
                FlushCore(element, makeProgress);
            IsFlushing = false;
        }

        internal abstract void FlushCore(T element, bool makeProgress);
    }
}
