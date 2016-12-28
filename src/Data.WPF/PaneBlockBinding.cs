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

        private List<PaneEntry<BlockBinding>> _entries = new List<PaneEntry<BlockBinding>>();

        public void AddChild<T>(BlockBinding<T> binding, string name)
            where T : UIElement, new()
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            VerifyNotSealed();
            _entries.Add(new PaneEntry<BlockBinding>(binding, name));
        }

        internal abstract Pane CreatePane();

        private Pane Create()
        {
            var result = CreatePane();
            foreach (var entry in _entries)
            {
                var binding = entry.Binding;
                binding.BeginSetup();
                var name = entry.Name;
                result.AddChild(binding.GetSettingUpElement(), name);
            }
            return result;
        }

        private Pane _settingUpPane;
        private List<Pane> _cachedPanes;

        internal sealed override UIElement GetSettingUpElement()
        {
            return _settingUpPane;
        }

        internal sealed override void SetSettingUpElement(UIElement value)
        {
            _settingUpPane = (Pane)value;
        }

        internal override void BeginSetup()
        {
            _settingUpPane = CachedList.GetOrCreate(ref _cachedPanes, Create);
            if (!_settingUpPane.IsNew)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    var childBinding = _entries[i].Binding;
                    childBinding.SetSettingUpElement(_settingUpPane.Children[i]);
                }
            }
        }

        internal override UIElement Setup(BlockView blockView)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var childBinding = _entries[i].Binding;
                childBinding.Setup(blockView);
            }
            return _settingUpPane;
        }

        internal override void Refresh(UIElement element)
        {
            var pane = (Pane)element;
            for (int i = 0; i < _entries.Count; i++)
            {
                var childBinding = _entries[i].Binding;
                var childElement = pane.Children[i];
                childBinding.Refresh(childElement);
            }
        }

        internal override void Cleanup(UIElement element, bool recycle)
        {
            var pane = (Pane)element;
            for (int i = 0; i < _entries.Count; i++)
            {
                var childBinding = _entries[i].Binding;
                childBinding.Cleanup(pane.Children[i], false);
            }

            if (recycle)
                CachedList.Recycle(ref _cachedPanes, pane);
        }

        internal override void EndSetup()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var childBinding = _entries[i].Binding;
                childBinding.EndSetup();
            }
            _settingUpPane.ClearIsNew();
            _settingUpPane = null;
        }
    }
}
