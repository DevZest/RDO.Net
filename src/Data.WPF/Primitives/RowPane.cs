using DevZest.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Windows.Primitives
{
    public abstract class RowPane : RowBinding
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

        internal abstract Pane CreatePane();

        private Pane Create()
        {
            return CreatePane().InitChildren(_bindings, _names);
        }

        private Pane _settingUpPane;

        internal sealed override UIElement GetSettingUpElement()
        {
            return _settingUpPane;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            _settingUpPane = value == null ? Create() : (Pane)value;
            _settingUpPane.BeginSetup(_bindings);
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(rowPresenter);
            return _settingUpPane;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((Pane)element).Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var pane = (Pane)element;
            pane.Cleanup(_bindings);
        }

        internal sealed override void EndSetup()
        {
            _settingUpPane.EndSetup(_bindings);
            _settingUpPane = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((Pane)element).FlushInput(_bindings);
        }
    }
}
