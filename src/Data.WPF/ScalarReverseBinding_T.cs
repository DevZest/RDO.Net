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
        private IScalarSet _scalars = ScalarSet.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        internal void Attach(T element)
        {
            _flushTrigger.Attach(element);
        }

        internal void Detach(T element)
        {
            _flushTrigger.Detach(element);
        }

        internal override IReadOnlyList<object> GetErrors()
        {
            return ValidationManager.GetMessages(this);
        }

        private Func<T, ReverseBindingMessage> _preValidator;
        public Func<T, ReverseBindingMessage> PreValidtor
        {
            get { return _preValidator; }
            set
            {
                VerifyNotSealed();
                _preValidator = value;
            }
        }

        private Func<ReverseBindingMessage> _postValidator;
        public Func<ReverseBindingMessage> PostValidator
        {
            get { return _postValidator; }
            set
            {
                VerifyNotSealed();
                _postValidator = value;
            }
        }

        public ScalarReverseBinding<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));
            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            VerifyNotSealed();
            _scalars = _scalars.Merge(scalar);
            _flushFuncs.Add((element) => scalar.SetValue(getValue(element)));
            return this;
        }

        internal override IScalarSet Scalars
        {
            get { return _scalars; }
        }

        private ReverseBindingMessage GetFlushingMessage(T element)
        {
            Debug.Assert(Binding != null && element.GetBinding() == Binding);
            return _preValidator == null ? ReverseBindingMessage.Empty : _preValidator(element);
        }

        internal bool IsDirty { get; private set; }

        internal void Flush(T element)
        {
            var message = GetFlushingMessage(element);
            var flushingErrorChanged = ValidationManager.UpdateFlushingMessage(this, message);
            if (message.IsEmpty || message.Severity == ValidationSeverity.Warning)
            {
                foreach (var flush in _flushFuncs)
                {
                    var flushed = flush(element);
                    if (flushed)
                        IsDirty = true;
                }
            }
            if (flushingErrorChanged)
                OnErrorsChanged();
        }
    }
}
