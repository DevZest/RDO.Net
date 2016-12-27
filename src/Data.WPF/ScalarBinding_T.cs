using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarBinding<T> : ScalarBinding
        where T : UIElement, new()
    {
        private ScalarInput<T> _input;
        public ScalarInput<T> Input
        {
            get { return _input; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                VerifyNotSealed();
                value.Seal(this);
                _input = value;
            }
        }

        private IValidationSource<Scalar> _validationSource = ValidationSource<Scalar>.Empty;
        internal sealed override IValidationSource<Scalar> ValidationSource
        {
            get { return Input != null ? Input.SourceScalars : _validationSource; }
        }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        public T SettingUpElement { get; private set; }

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup()
        {
            Debug.Assert(SettingUpElement != null);
            Setup(SettingUpElement);
            Refresh(SettingUpElement);
            if (Input != null)
                Input.Attach(SettingUpElement);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        protected virtual void Setup(T element)
        {
        }

        protected abstract void Refresh(T element);

        protected virtual void Cleanup(T element)
        {
        }

        internal sealed override void Refresh(UIElement element)
        {
            Refresh((T)element);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }

        internal sealed override bool HasPreValidatorError
        {
            get { return Input == null ? false : Input.HasPreValidatorError; }
        }

        internal sealed override bool HasAsyncValidator
        {
            get { return Input == null ? false : Input.HasAsyncValidator; }
        }

        internal sealed override void RunAsyncValidator()
        {
            Debug.Assert(HasAsyncValidator);
            Input.RunAsyncValidator();
        }
    }
}
