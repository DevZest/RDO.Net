using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarReverseBinding<T> : ReverseBinding<T>
        where T : UIElement, new()
    {
        internal static ScalarReverseBinding<T> Create<TData>(Trigger<T> flushTrigger, Scalar<TData> scalar, Func<T, TData> dataGetter)
        {
            return new ScalarReverseBinding<T>(flushTrigger).Bind(scalar, dataGetter);
        }

        private ScalarReverseBinding(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        private IScalarSet _scalars = ScalarSet.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();
        private Func<ValidationMessage> _postValidator;

        public ScalarReverseBinding<T> WithPreValidator(Func<T, ValidationMessage> preValidator, Trigger<T> preValidatorTrigger = null)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public ScalarReverseBinding<T> WithPostValidator(Func<ValidationMessage> postValidator)
        {
            VerifyNotSealed();
            _postValidator = postValidator;
            return this;
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

        internal IScalarSet Scalars
        {
            get { return _scalars; }
        }

        internal override bool DoFlush(T element)
        {
            bool result = false;
            foreach (var flush in _flushFuncs)
            {
                var flushed = flush(element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        internal override void RefreshValidationMessages()
        {
            ValidationManager.RefreshScalarValidationMessages();
        }

        internal override IEnumerable<ValidationMessage> GetValidationMessages(ValidationSeverity severity)
        {
            return ValidationManager.GetValidationMessages(this, severity);
        }

        internal override IEnumerable<ValidationMessage> GetMergedValidationMessages(ValidationSeverity severity)
        {
            return ValidationManager.GetMergedValidationMessages(this, severity);
        }
    }
}
