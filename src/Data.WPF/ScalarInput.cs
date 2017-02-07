using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using DevZest.Data.Primitives;

namespace DevZest.Data.Windows
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

        public ScalarInput<T> Bind<TData>(Scalar<TData> scalar, Func<T, TData> getValue)
        {
            if (scalar == null)
                throw new ArgumentNullException(nameof(scalar));

            VerifyNotSealed();
            _flushFuncs.Add(element =>
            {
                if (getValue == null)
                    return false;
                var value = getValue(element);
                if (Comparer<TData>.Default.Compare(scalar.Value, value) == 0)
                    return false;
                else
                {
                    scalar.Value = value;
                    return true;
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

        private Action<T, ViewInputError> _onRefresh;
        internal void Refresh(T element)
        {
            var inputError = GetInputError(element);
            if (_onRefresh != null)
                _onRefresh(element, inputError);
            element.RefreshValidation(inputError, AbstractValidationMessageGroup.Empty);
        }

        public ScalarInput<T> WithRefreshAction(Action<T, ViewInputError> onRefresh)
        {
            VerifyNotSealed();
            _onRefresh = onRefresh;
            return this;
        }
    }
}
