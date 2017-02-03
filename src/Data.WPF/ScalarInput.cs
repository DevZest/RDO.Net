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
    public interface IScalarInput
    {
    }

    public sealed class ScalarInput<T> : Input<T>, IScalarInput
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

        public sealed override TwoWayBinding Binding
        {
            get { return ScalarBinding; }
        }

        internal void Seal(ScalarBinding<T> scalarBinding)
        {
            Debug.Assert(scalarBinding != null);
            VerifyNotSealed();
            ScalarBinding = scalarBinding;
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
    }
}
