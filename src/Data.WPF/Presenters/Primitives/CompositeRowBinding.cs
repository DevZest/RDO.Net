using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeRowBinding : RowBinding, ICompositeBinding
    {
        private List<RowBinding> _bindings = new List<RowBinding>();
        private List<string> _names = new List<string>();

        IReadOnlyList<Binding> ICompositeBinding.Bindings
        {
            get { return _bindings; }
        }

        IReadOnlyList<string> ICompositeBinding.Names
        {
            get { return _names; }
        }

        void ICompositeBinding.Setup<T>(T compositeView)
        {
            this.Verify(compositeView, nameof(compositeView));
            Setup(compositeView.GetRowPresenter());
        }

        internal void InternalAddChild<T>(RowBinding<T> binding, string name)
            where T : UIElement, new()
        {
            Binding.VerifyAdding(binding, nameof(binding));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            VerifyNotSealed();
            _bindings.Add(binding);
            _names.Add(name);
            binding.Seal(this, _bindings.Count - 1);
        }

        internal abstract ICompositeView CreateView();

        private ICompositeView Initialize()
        {
            return CreateView().BindingDispatcher.Initialize(this);
        }

        private ICompositeView _settingUpView;

        internal sealed override UIElement GetSettingUpElement()
        {
            return (UIElement)_settingUpView;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            if (value == null)
                _settingUpView = Initialize();
            else
            {
                _settingUpView = (ICompositeView)value;
                _settingUpView.BindingDispatcher.BeginSetup();
            }
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(rowPresenter);
            return (UIElement)_settingUpView;
        }

        private bool _isRefreshing;
        internal override bool IsRefreshing
        {
            get { return _isRefreshing; }
        }

        internal sealed override void Refresh(UIElement element)
        {
            _isRefreshing = true;
            ((ICompositeView)element).BindingDispatcher.Refresh();
            _isRefreshing = false;
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var view = (ICompositeView)element;
            view.BindingDispatcher.Cleanup();
        }

        internal sealed override void EndSetup()
        {
            _settingUpView.BindingDispatcher.EndSetup();
            _settingUpView = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((ICompositeView)element).BindingDispatcher.FlushInput();
        }
    }
}
