using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarReverseBinding<T> : ScalarReverseBinding
        where T : UIElement, new()
    {
        internal static ScalarReverseBinding<T> Create<TData>(Trigger<T> flushTrigger, Scalar<TData> scalar, Func<T, TData> dataGetter)
        {
            return new ScalarReverseBinding<T>(flushTrigger).Bind(scalar, dataGetter);
        }

        private ScalarReverseBinding(Trigger<T> flushTrigger)
        {
            _flushTrigger = flushTrigger;
        }

        private Trigger<T> _flushTrigger;
        private Trigger<T> _flushErrorTrigger;
        private IScalarSet _scalars = ScalarSet.Empty;
        private List<Action<T>> _flushActions = new List<Action<T>>();
        private Func<T, ReverseBindingError> _flushErrorFunc;

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
            if (_flushErrorTrigger != null)
                _flushErrorTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
            if (_flushErrorTrigger != null)
                _flushErrorTrigger.Detach(element);
        }

        internal override IReadOnlyList<object> GetErrors()
        {
            return ValidationManager.GetErrors(this);
        }

        public ScalarReverseBinding<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> dataGetter)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));
            if (dataGetter == null)
                throw new ArgumentNullException(nameof(dataGetter));

            VerifyNotSealed();
            _scalars = _scalars.Merge(scalar);
            _flushActions.Add((element) => scalar.Value = dataGetter(element));
            return this;
        }

        public ScalarReverseBinding<T> FlushError(Func<T, ReverseBindingError> flushErrorFunc, Trigger<T> flushErrorTrigger = null)
        {
            VerifyNotSealed();
            _flushErrorFunc = flushErrorFunc;
            if (flushErrorTrigger != null)
            {
                flushErrorTrigger.Initialize(this.FlushError);
                _flushErrorTrigger = flushErrorTrigger;
            }
            return this;
        }

        internal override IScalarSet Scalars
        {
            get { return _scalars; }
        }

        private ReverseBindingError GetError(T element)
        {
            Debug.Assert(Binding != null && element.GetBinding() == Binding);
            return _flushErrorFunc == null ? ReverseBindingError.Empty : _flushErrorFunc(element);
        }

        internal void Flush(T element)
        {
            var error = GetError(element);
            var flushingErrorChanged = ValidationManager.UpdateFlushingError(this, error);
            if (error.IsEmpty)
            {
                foreach (var flushAction in _flushActions)
                    flushAction(element);
            }
            if (flushingErrorChanged)
                OnErrorsChanged();
        }

        private void FlushError(T element)
        {
            var error = GetError(element);
            var flushingErrorChanged = ValidationManager.UpdateFlushingError(this, error);
            if (flushingErrorChanged)
                OnErrorsChanged();
        }
    }
}
