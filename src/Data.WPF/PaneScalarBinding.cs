using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class PaneScalarBinding : ScalarBinding
    {
        private sealed class ConcretePaneScalarBinding<T> : PaneScalarBinding
            where T : Pane, new()
        {
            internal override Pane CreatePane()
            {
                return new T();
            }
        }

        public static PaneScalarBinding Create<T>()
            where T : Pane, new()
        {
            return new ConcretePaneScalarBinding<T>();
        }

        private List<ScalarBinding> _bindings = new List<ScalarBinding>();
        private List<string> _names = new List<string>();

        public void AddChild<T>(ScalarBinding<T> binding, string name)
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

        private Pane[] Create(int startOffset)
        {
            _settingUpStartOffset = startOffset;

            if (startOffset == BlockDimensions)
                return Array<Pane>.Empty;

            var count = BlockDimensions - startOffset;
            var result = new Pane[count];
            for (int i = 0; i < count; i++)
                result[i] = Create();
            return result;
        }

        private Pane Create()
        {
            var result = CreatePane().InitChildren(_bindings, _names);
            OnCreated(result);
            return result;
        }

        private int _settingUpStartOffset;
        private Pane[] _settingUpPanes;
        private IReadOnlyList<Pane> SettingUpPanes
        {
            get { return _settingUpPanes; }
        }

        private Pane SettingUpPane { get; set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            Debug.Assert(!IsMultidimensional);
            return SettingUpPane;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (IsMultidimensional)
            {
                _settingUpPanes = Create(startOffset);
                for (int i = 0; i < SettingUpPanes.Count; i++)
                    SettingUpPanes[i].BeginSetup(_bindings);
            }
            else if (startOffset == 0)
            {
                SettingUpPane = Create();
                SettingUpPane.BeginSetup(_bindings);
            }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!IsMultidimensional);
            SettingUpPane = value == null ? Create() : (Pane)value;
            SettingUpPane.BeginSetup(_bindings);
        }

        internal sealed override UIElement Setup(int blockDimension)
        {
            if (IsMultidimensional)
            {
                Debug.Assert(SettingUpPanes != null);
                SettingUpPane = SettingUpPanes[blockDimension - _settingUpStartOffset];
            }

            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(blockDimension);
            return SettingUpPane;
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
            for (int i = 0; i < SettingUpPanes.Count; i++)
                SettingUpPanes[i].EndSetup(_bindings);
            _settingUpPanes = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((Pane)element).FlushInput(_bindings);
        }
    }
}
