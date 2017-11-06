using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using DevZest.Data;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarInput<T> : Input<T>
        where T : UIElement, new()
    {
        internal  ScalarInput(ScalarBinding<T> scalarBinding, Trigger<T> flushTrigger)
            : base(flushTrigger)
        {
            Debug.Assert(scalarBinding != null);
            ScalarBinding = scalarBinding;
        }

        public ScalarBinding<T> ScalarBinding { get; private set; }

        public sealed override TwoWayBinding Binding
        {
            get { return ScalarBinding; }
        }

        public sealed override FlushErrorMessage GetFlushError(UIElement element)
        {
            return InputManager.GetScalarFlushError(element);
        }

        internal sealed override void SetFlushError(UIElement element, FlushErrorMessage inputError)
        {
            InputManager.SetScalarFlushError(element, inputError);
        }

        public ScalarInput<T> WithFlushValidator(Func<T, string> flushValidator, string flushValidatorId = null)
        {
            SetFlushValidator(flushValidator, flushValidatorId);
            return this;
        }

        internal IScalars Target { get; private set; } = Scalars.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        public ScalarInput<T> WithFlush<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));

            VerifyNotSealed();
            Target = Target.Union(scalar);
            _flushFuncs.Add(element =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);
                return scalar.ChangeValue(value);
            });
            return this;
        }

        internal override void FlushCore(T element)
        {
            var flushed = DoFlush(element);
            if (flushed)
                InputManager.MakeProgress(this);
        }

        private bool DoFlush(T element)
        {
            bool result = false;
            for (int i = 0; i < _flushFuncs.Count; i++)
            {
                var flush = _flushFuncs[i];
                var flushed = flush(element);
                if (flushed)
                    result = true;
            }
            return result;
        }

        private Action<T, ScalarPresenter> _onRefresh;
        internal void Refresh(T element, ScalarPresenter scalarPresenter)
        {
            if (!IsFlushing && GetFlushError(element) == null)
                ScalarBinding.Refresh(element, scalarPresenter);
            var flushError = GetFlushError(element);
            if (_onRefresh != null)
            {
                scalarPresenter.SetErrors(flushError, null);
                _onRefresh(element, scalarPresenter);
                scalarPresenter.SetErrors(null, null);
            }
            RefreshValidation(element);
        }

        private void RefreshValidation(T element)
        {
            element.RefreshValidation(() => GetFlushError(element), () => Errors, () => Warnings);
        }

        public IScalarValidationMessages Errors
        {
            get
            {
                var result = ScalarValidationMessages.Empty;
                result = AddValidationMessages(result, InputManager.ScalarValidationErrors, x => IsVisible(x, true));
                result = AddAsyncValidationMessages(result, ValidationSeverity.Error);
                result = AddValidationMessages(result, InputManager.AssignedScalarValidationResults, x => x.Severity == ValidationSeverity.Error && IsVisible(x, false));
                return result;
            }
        }

        public IScalarValidationMessages Warnings
        {
            get
            {
                var result = ScalarValidationMessages.Empty;
                result = AddValidationMessages(result, InputManager.ScalarValidationWarnings, x => IsVisible(x, true));
                result = AddAsyncValidationMessages(result, ValidationSeverity.Warning);
                result = AddValidationMessages(result, InputManager.AssignedScalarValidationResults, x => x.Severity == ValidationSeverity.Warning && IsVisible(x, false));
                return result;
            }
        }

        private bool IsVisible(ScalarValidationMessage validationMessage, bool progressVisible)
        {
            var source = validationMessage.Source;
            return source.Overlaps(Target) && InputManager.ScalarValidationProgress.IsVisible(source) == progressVisible;
        }

        private static IScalarValidationMessages AddValidationMessages(IScalarValidationMessages result, IScalarValidationMessages messages, Func<ScalarValidationMessage, bool> predict)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (predict(message))
                    result = result.Add(message);
            }
            return result;
        }

        private IScalarValidationMessages AddAsyncValidationMessages(IScalarValidationMessages result, ValidationSeverity severity)
        {
            var asyncValidators = Template.ScalarAsyncValidators;
            for (int i = 0; i < asyncValidators.Count; i++)
            {
                var asyncValidator = asyncValidators[i];
                var messages = severity == ValidationSeverity.Error ? asyncValidator.Errors : asyncValidator.Warnings;
                result = AddValidationMessages(result, messages, x => IsVisible(x, true));
            }

            return result;
        }

        public ScalarInput<T> WithRefreshAction(Action<T, ScalarPresenter> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

        public ScalarBinding<T> EndInput()
        {
            return ScalarBinding;
        }

        public ScalarInput<T> AddAsyncValidator(Func<Task<IScalarValidationMessages>> action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            VerifyNotSealed();

            var asyncValidator = ScalarAsyncValidator.Create<T>(this, action, postAction);
            Template.InternalScalarAsyncValidators = Template.InternalScalarAsyncValidators.Add(asyncValidator);
            return this;
        }
    }
}
