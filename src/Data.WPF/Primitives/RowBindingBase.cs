using DevZest.Data.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
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

        internal sealed override IValidationSource<Column> ValidationSource
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

        protected abstract void Setup(T element, RowPresenter rowPresenter);

        protected abstract void Refresh(T element, RowPresenter rowPresenter);

        protected abstract void Cleanup(T element, RowPresenter rowPresenter);

        internal sealed override void Refresh(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.SetDataErrorInfo(e, rowPresenter);
            Refresh(e, rowPresenter);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
            CachedList.Recycle(ref _cachedElements, e);
        }

        internal sealed override bool ShouldRefresh(UIElement element)
        {
            if (_input == null)
                return true;

            var rowPresenter = element.GetRowPresenter();
            Debug.Assert(rowPresenter != null);
            return rowPresenter != Template.ElementManager.CurrentRow;
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

        internal sealed override bool HasAsyncValidator
        {
            get { return Input == null ? false : Input.HasAsyncValidator; }
        }

        internal sealed override void RunAsyncValidator(RowPresenter rowPresenter)
        {
            Debug.Assert(Input != null);
            Input.RunAsyncValidator(rowPresenter);
        }
    }
}
