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
        internal  ScalarInput(ScalarBinding<T> scalarBinding, Trigger<T> flushTrigger, Action<T> flushAction)
            : base(flushTrigger)
        {
            Debug.Assert(scalarBinding != null);
            ScalarBinding = scalarBinding;

            if (flushAction == null)
                throw new ArgumentNullException(nameof(flushAction));
            _flushAction = flushAction;
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

        private readonly Action<T> _flushAction;

        public ScalarInput<T> WithInputValidator(Func<T, InputError> inputValidator, Trigger<T> inputValidationTrigger = null)
        {
            SetInputValidator(inputValidator, inputValidationTrigger);
            return this;
        }

        internal override void FlushCore(T element)
        {
            _flushAction(element);
        }

        private Action<T, ViewInputError> _onRefresh;
        internal void Refresh(T element)
        {
            var inputError = GetInputError(element);
            if (_onRefresh != null)
                _onRefresh(element, inputError);
            else if (inputError == null)
                ScalarBinding.Refresh(element);
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
