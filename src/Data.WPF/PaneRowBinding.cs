using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class PaneRowBinding : RowBinding
    {
        private sealed class ConcretePaneRowBinding<T> : PaneRowBinding
            where T : Pane, new()
        {
            internal override Pane CreatePane()
            {
                return new T();
            }
        }

        public static PaneRowBinding Create<T>()
            where T : Pane, new()
        {
            return new ConcretePaneRowBinding<T>();
        }

        private List<RowBinding> _bindings = new List<RowBinding>();
        private List<string> _names = new List<string>();

        public void AddChild<T>(RowBinding<T> binding, string name)
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

        internal sealed override void OnRowDisposed(RowPresenter rowPresenter)
        {
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].OnRowDisposed(rowPresenter);
        }
    }
}
