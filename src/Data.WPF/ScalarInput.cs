using DevZest.Data.Windows.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using DevZest.Data.Primitives;

namespace DevZest.Data.Windows
{
    public sealed class ScalarInput<T> : Input<T>
        where T : UIElement, new()
    {
        public ScalarInput(Trigger<T> flushTrigger, Action<T> flushAction)
            : base(flushTrigger)
        {
            if (flushAction == null)
                throw new ArgumentNullException(nameof(flushAction));
            _flushAction = flushAction;
        }

        public ScalarBinding<T> ScalarBinding { get; private set; }

        public sealed override Binding Binding
        {
            get { return ScalarBinding; }
        }

        internal void Seal(ScalarBinding<T> scalarBinding)
        {
            Debug.Assert(scalarBinding != null);
            VerifyNotSealed();
            ScalarBinding = scalarBinding;
        }

        internal sealed override ViewInputError GetInputError(UIElement element)
        {
            return ValidationManager.GetScalarInputError(element);
        }

        internal sealed override void SetInputError(UIElement element, ViewInputError inputError)
        {
            ValidationManager.SetScalarInputError(element, inputError);
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

        internal void Refresh(T element)
        {
            if (GetInputError(element) == null)
                ScalarBinding.Refresh(element);
            element.RefreshValidation(GetInputError(element), AbstractValidationMessageGroup.Empty);
        }
    }
}
