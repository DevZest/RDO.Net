using System;
using System.Windows;
using DevZest.Data.Windows.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.Windows
{
    public abstract class PaneBlockBinding : BlockBinding
    {
        private sealed class ConcretePaneBlockBinding<T> : PaneBlockBinding
            where T : Pane, new()
        {
            internal override Pane CreatePane()
            {
                return new T();
            }
        }

        public static PaneBlockBinding Create<T>()
            where T : Pane, new()
        {
            return new ConcretePaneBlockBinding<T>();
        }

        private List<BlockBinding> _bindings = new List<BlockBinding>();
        private List<string> _names = new List<string>();

        public void AddChild<T>(BlockBinding<T> binding, string name)
            where T : UIElement, new()
        {
            Binding.VerifyAdding(binding, nameof(binding));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            VerifyNotSealed();
            _bindings.Add(binding);
            _names.Add(name);
            binding.ParentBinding = this;
        }

        internal abstract Pane CreatePane();

        private Pane Create()
        {
            return CreatePane().BeginSetup(_bindings, _names);
        }

        private Pane _settingUpPane;
        private List<Pane> _cachedPanes;

        internal sealed override UIElement GetSettingUpElement()
        {
            return _settingUpPane;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            _settingUpPane = (Pane)value;
        }

        internal sealed override void BeginSetup()
        {
            _settingUpPane = CachedList.GetOrCreate(ref _cachedPanes, Create).BeginSetup(_bindings);
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(blockView);
            return _settingUpPane;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((Pane)element).Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element, bool recycle)
        {
            var pane = (Pane)element;
            pane.Cleanup(_bindings);
            if (recycle)
                CachedList.Recycle(ref _cachedPanes, pane);
        }

        internal sealed override void EndSetup()
        {
            _settingUpPane.EndSetup(_bindings);
            _settingUpPane = null;
        }
    }
}
