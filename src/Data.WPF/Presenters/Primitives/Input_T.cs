using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class Input<TBinding, TTarget>
        where TBinding : TwoWayBinding
    {
        public abstract TBinding Binding { get; }
        public abstract TTarget Target { get; }
        public int Index { get; internal set; } = -1;
        internal abstract bool IsPrecedingOf(Input<TBinding, TTarget> input);

        internal bool IsPlaceholder
        {
            get { return typeof(ValidationPlaceholder).IsAssignableFrom(Binding.ViewType); }
        }
    }

    public abstract class Input<T, TBinding, TTarget> : Input<TBinding, TTarget>
        where T : UIElement, new()
        where TBinding : TwoWayBinding
    {
        internal Input(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger)
        {
            if (flushingTrigger == null)
                throw new ArgumentNullException(nameof(flushingTrigger));
            VerifyNotInitialized(flushingTrigger, nameof(flushingTrigger));
            _flushingTrigger = flushingTrigger;
            _flushingTrigger.ExecuteAction = Flush;

            if (progressiveFlushingTrigger != null)
            {
                _progressiveFlushingTrigger = progressiveFlushingTrigger;
                _progressiveFlushingTrigger.ExecuteAction = ProgressiveFlush;
            }
        }

        private void VerifyNotInitialized(Trigger<T> trigger, string paramName)
        {
            if (trigger.ExecuteAction != null)
                throw new ArgumentException(DiagnosticMessages.Input_TriggerAlreadyInitialized, paramName);
        }

        private readonly Trigger<T> _flushingTrigger;
        private readonly Trigger<T> _progressiveFlushingTrigger;
        private Func<T, string> _flushingValidator;

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

        internal void SetFlushingValidator(Func<T, string> flushingValidator)
        {
            if (flushingValidator == null)
                throw new ArgumentNullException(nameof(flushingValidator));

            _flushingValidator = flushingValidator;
        }

        internal void Attach(T element)
        {
            _flushingTrigger.Attach(element);
            if (_progressiveFlushingTrigger != null)
                _progressiveFlushingTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushingTrigger.Detach(element);
            if (_progressiveFlushingTrigger != null)
                _progressiveFlushingTrigger.Detach(element);
        }

        public abstract FlushingError GetFlushingError(UIElement element);

        internal abstract void SetFlushingError(UIElement element, FlushingError flushingError);

        internal void ValidateFlush(T element)
        {
            if (_flushingValidator == null)
                return;
            var oldflushingError = GetFlushingError(element);
            var flushingErrorDescription = _flushingValidator(element);
            if (IsFlushingErrorChanged(flushingErrorDescription, oldflushingError))
                SetFlushingError(element, string.IsNullOrEmpty(flushingErrorDescription) ? null : new FlushingError(flushingErrorDescription, element));
        }

        private static bool IsFlushingErrorChanged(string flushingErrorMessage, FlushingError flushingError)
        {
            return string.IsNullOrEmpty(flushingErrorMessage) ? flushingError != null
                : flushingError == null || flushingError.Message != flushingErrorMessage;
        }

        public bool IsFlushing { get; private set; }

        internal void Flush(T element)
        {
            PerformFlush(element, _progressiveFlushingTrigger == null || IsValidationVisible);
        }

        internal abstract bool IsValidationVisible { get; }

        private void ProgressiveFlush(T element)
        {
            PerformFlush(element, true);
        }

        private void PerformFlush(T element, bool makeProgress)
        {
            if (Binding.IsRefreshing)
                return;

            IsFlushing = true;
            ValidateFlush(element);
            if (GetFlushingError(element) == null)
                FlushCore(element, makeProgress);
            IsFlushing = false;
        }

        internal abstract void FlushCore(T element, bool makeProgress);
    }
}
