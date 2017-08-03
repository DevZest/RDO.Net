using System;
using System.Windows;
using System.Collections.Generic;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeBlockBinding : BlockBinding
    {
        private List<BlockBinding> _bindings = new List<BlockBinding>();
        private List<string> _names = new List<string>();

        internal void InternalAddChild<T>(BlockBinding<T> binding, string name)
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
            return CreateView().BindingDispatcher.InitChildren(_bindings, _names);
        }

        private ICompositeView _settingUpView;
        private List<ICompositeView> _cachedViews;

        internal sealed override UIElement GetSettingUpElement()
        {
            return (UIElement)_settingUpView;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            _settingUpView = value == null ? Create() : (ICompositeView)value;
            _settingUpView.BindingDispatcher.BeginSetup(_bindings);
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(blockView);
            return (UIElement)_settingUpView;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((ICompositeView)element).BindingDispatcher.Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var view = (ICompositeView)element;
            view.BindingDispatcher.Cleanup(_bindings);
        }

        internal sealed override void EndSetup()
        {
            _settingUpView.BindingDispatcher.EndSetup(_bindings);
            _settingUpView = null;
        }
    }
}
