using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class RowBinding<T> : RowBinding
        where T : UIElement, new()
    {
        private RowInput<T> _input;
        public RowInput<T> Input
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

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        private IValidationSource<Column> ValidationSource
        {
            get { return Input == null ? ValidationSource<Column>.Empty : Input.SourceColumns; }
        }

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            SettingUpElement = (T)value;
        }

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetRowPresenter(rowPresenter);
            Setup(SettingUpElement, rowPresenter);
            Refresh(SettingUpElement, rowPresenter);
            if (Input != null)
                Input.Attach(SettingUpElement);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        protected virtual void Setup(T element, RowPresenter rowPresenter)
        {
        }

        protected abstract void Refresh(T element, RowPresenter rowPresenter);

        protected virtual void Cleanup(T element, RowPresenter rowPresenter)
        {
        }

        internal sealed override void Refresh(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.SetDataErrorInfo(e, rowPresenter);
            Refresh(e, rowPresenter);
        }

        internal sealed override void Cleanup(UIElement element, bool recycle)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            if (recycle)
                CachedList.Recycle(ref _cachedElements, e);
        }

        internal sealed override bool HasPreValidatorError
        {
            get { return Input == null ? false : Input.HasPreValidatorError; }
        }

        private ValidationManager ValidationManager
        {
            get { return Template.ValidationManager; }
        }

        internal sealed override void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (Input != null)
                Input.OnRowDisposed(rowPresenter);
        }

        private bool HasAsyncValidator
        {
            get { return Input == null ? false : Input.HasAsyncValidator; }
        }

        internal sealed override void RunAsyncValidatorIfNecessary(RowPresenter rowPresenter)
        {
            Debug.Assert(Input != null);
            if (ShouldRunAsyncValidator(rowPresenter))
                Input.RunAsyncValidator(rowPresenter);
        }

        private bool ShouldRunAsyncValidator(RowPresenter rowPresenter)
        {
            if (!ValidationManager.IsVisible(rowPresenter, ValidationSource))
                return false;

            return HasAsyncValidator && !HasPreValidatorError && ValidationManager.HasNoError(rowPresenter, ValidationSource);
        }
    }
}
