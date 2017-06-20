using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using DevZest.Data;

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

        internal sealed override ViewInputError GetInputError(UIElement element)
        {
            return InputManager.GetScalarInputError(element);
        }

        internal sealed override void SetInputError(UIElement element, ViewInputError inputError)
        {
            InputManager.SetScalarInputError(element, inputError);
        }

        public ScalarInput<T> WithInputValidator(Func<T, InputError> inputValidator, Trigger<T> inputValidationTrigger = null)
        {
            SetInputValidator(inputValidator, inputValidationTrigger);
            return this;
        }

        private List<Func<T, bool>> _flushFuncs = new List<Func<T, bool>>();

        public ScalarInput<T> WithFlush<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));

            VerifyNotSealed();
            _flushFuncs.Add(element =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);

                var inputError = scalar.Validate(value);
                if (inputError.IsEmpty)
                    return scalar.ChangeValue(value);
                else
                {
                    InputManager.SetScalarValueError(element, new ViewInputError(inputError, element));
                    return false;
                }
            });
            return this;
        }

        internal override void FlushCore(T element)
        {
            DoFlush(element);
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
            var inputError = GetInputError(element);
            var valueError = InputManager.GetScalarValueError(element);
            if (_onRefresh != null)
            {
                scalarPresenter.SetErrors(inputError, valueError);
                _onRefresh(element, scalarPresenter);
                scalarPresenter.SetErrors(null, null);
            }
            element.RefreshValidation(inputError ?? valueError, AbstractValidationMessageGroup.Empty);
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
    }
}
