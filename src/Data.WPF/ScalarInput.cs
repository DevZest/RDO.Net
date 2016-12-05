using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal static ScalarInput<T> Create<TData>(Trigger<T> flushTrigger, Scalar<TData> scalar, Func<T, TData> getValue)
        {
            return new ScalarInput<T>(flushTrigger).Bind(scalar, getValue);
        }

        private ScalarInput(Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
        }

        private IScalarSet _scalars = ScalarSet.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();
        private Func<Task<ValidationMessage>> _asyncValidator;

        public ScalarInput<T> WithPreValidator(Func<T, ValidationMessage> preValidator, Trigger<T> preValidatorTrigger = null)
        {
            SetPreValidator(preValidator, preValidatorTrigger);
            return this;
        }

        public ScalarInput<T> WithAsyncValidator(Func<Task<ValidationMessage>> asyncValidator)
        {
            VerifyNotSealed();
            _asyncValidator = asyncValidator;
            return this;
        }

        public ScalarInput<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
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
