using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeRowBinding : RowBinding
    {
        private List<RowBinding> _bindings = new List<RowBinding>();
        private List<string> _names = new List<string>();

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

        private ICompositeView Create()
        {
            return CreateView().CompositeBinding.InitChildren(_bindings, _names);
        }

        private ICompositeView _settingUpView;

        internal sealed override UIElement GetSettingUpElement()
        {
            return (UIElement)_settingUpView;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            _settingUpView = value == null ? Create() : (ICompositeView)value;
            _settingUpView.CompositeBinding.BeginSetup(_bindings);
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(rowPresenter);
            return (UIElement)_settingUpView;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((ICompositeView)element).CompositeBinding.Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var view = (ICompositeView)element;
            view.CompositeBinding.Cleanup(_bindings);
        }

        internal sealed override void EndSetup()
        {
            _settingUpView.CompositeBinding.EndSetup(_bindings);
            _settingUpView = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((ICompositeView)element).CompositeBinding.FlushInput(_bindings);
        }
    }
}
