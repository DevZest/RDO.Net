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
        internal  ScalarInput(ScalarBinding<T> scalarBinding, Trigger<T> flushTrigger, Trigger<T> progressiveFlushTrigger)
            : base(flushTrigger, progressiveFlushTrigger)
        {
            Debug.Assert(scalarBinding != null);
            ScalarBinding = scalarBinding;
        }

        public ScalarBinding<T> ScalarBinding { get; private set; }

        public sealed override TwoWayBinding Binding
        {
            get { return ScalarBinding; }
        }

        private ScalarValidation ScalarValidation
        {
            get { return InputManager.ScalarValidation; }
        }

        public sealed override FlushErrorMessage GetFlushError(UIElement element)
        {
            return ScalarValidation.GetFlushError(element);
        }

        internal sealed override void SetFlushError(UIElement element, FlushErrorMessage inputError)
        {
            ScalarValidation.SetFlushError(element, inputError);
        }

        public ScalarInput<T> WithFlushValidator(Func<T, string> flushValidator)
        {
            SetFlushValidator(flushValidator);
            return this;
        }

        internal IScalars Target { get; private set; } = Scalars.Empty;
        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        public ScalarInput<T> WithFlush(Scalar scalar, Func<T, bool> flushFunc)
        {
            Check.NotNull(scalar, nameof(scalar));
            Check.NotNull(flushFunc, nameof(flushFunc));

            VerifyNotSealed();
            Target = Target.Union(scalar);
            _flushFuncs.Add(flushFunc);
            return this;
        }

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

        internal override bool IsValidationVisible
        {
            get { return InputManager.ScalarValidation.IsVisible(Target); }
        }

        internal override void FlushCore(T element, bool makeProgress)
        {
            var valueChanged = DoFlush(element);
            ScalarValidation.OnFlushed(this, makeProgress, valueChanged);
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
            element.RefreshValidation(() => GetFlushError(element), () => ValidationErrors, () => ValidationWarnings);
        }

        public IScalarValidationMessages ValidationErrors
        {
            get { return ScalarValidation.GetValidationErrors(Target); }
        }

        public IScalarValidationMessages ValidationWarnings
        {
            get { return ScalarValidation.GetValidationWarnings(Target); }
        }

        public ScalarInput<T> WithRefreshAction(Action<T, ScalarPresenter> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }

        public ScalarBinding<T> EndInput()
        {
            Target = Target.Seal();
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
