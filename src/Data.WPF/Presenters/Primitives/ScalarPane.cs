using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ScalarPane : ScalarBinding
    {
        private List<ScalarBinding> _bindings = new List<ScalarBinding>();
        private List<string> _names = new List<string>();

        internal void InternalAddChild<T>(ScalarBinding<T> binding, string name)
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

            if (startOffset == FlowCount)
                return Array<Pane>.Empty;

            var count = FlowCount - startOffset;
            var result = new Pane[count];
            for (int i = 0; i < count; i++)
                result[i] = CreatePane(startOffset + i);
            return result;
        }

        private Pane CreatePane(int flowIndex)
        {
            var result = CreatePane();
            result.SetScalarFlowIndex(flowIndex);
            ScalarPresenter.SetFlowIndex(flowIndex);
            result.InitChildren(_bindings, _names);
            OnCreated(result);
            ScalarPresenter.SetFlowIndex(-1);
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
            Debug.Assert(!Flowable);
            return SettingUpPane;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (Flowable)
            {
                _settingUpPanes = Create(startOffset);
                for (int i = 0; i < SettingUpPanes.Count; i++)
                    SettingUpPanes[i].BeginSetup(_bindings);
            }
            else if (startOffset == 0)
            {
                SettingUpPane = CreatePane(0);
                SettingUpPane.BeginSetup(_bindings);
            }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!Flowable);
            SettingUpPane = value == null ? CreatePane(0) : (Pane)value;
            SettingUpPane.BeginSetup(_bindings);
        }

        internal sealed override void PrepareSettingUpElement(int flowIndex)
        {
            if (Flowable)
            {
                Debug.Assert(SettingUpPanes != null);
                SettingUpPane = SettingUpPanes[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (Flowable)
                SettingUpPane = null;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            EnterSetup(flowIndex);

            var result = SettingUpPane;
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(flowIndex);

            ExitSetup();
            return result;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((Pane)element).Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (Pane)element;
            e.Cleanup(_bindings);
            e.SetScalarFlowIndex(0);
        }

        internal sealed override void EndSetup()
        {
            if (Flowable)
            {
                for (int i = 0; i < SettingUpPanes.Count; i++)
                    SettingUpPanes[i].EndSetup(_bindings);
            }
            else
                SettingUpPane.EndSetup(_bindings);
            _settingUpPanes = null;
            SettingUpPane = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((Pane)element).FlushInput(_bindings);
        }
    }
}
