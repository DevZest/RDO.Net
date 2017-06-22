using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeScalarBinding : ScalarBinding
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

        internal abstract ICompositeView CreateView();

        private ICompositeView[] Create(int startOffset)
        {
            _settingUpStartOffset = startOffset;

            if (startOffset == FlowCount)
                return Array<ICompositeView>.Empty;

            var count = FlowCount - startOffset;
            var result = new ICompositeView[count];
            for (int i = 0; i < count; i++)
                result[i] = CreateView(startOffset + i);
            return result;
        }

        private ICompositeView CreateView(int flowIndex)
        {
            var result = CreateView();
            var resultElement = (UIElement)result;
            resultElement.SetScalarFlowIndex(flowIndex);
            ScalarPresenter.SetFlowIndex(flowIndex);
            result.CompositeBinding.InitChildren(_bindings, _names);
            OnCreated(resultElement);
            ScalarPresenter.SetFlowIndex(-1);
            return result;
        }

        private int _settingUpStartOffset;
        private ICompositeView[] _settingUpViews;
        private IReadOnlyList<ICompositeView> SettingUpViews
        {
            get { return _settingUpViews; }
        }

        private ICompositeView SettingUpView { get; set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            Debug.Assert(!Flowable);
            return (UIElement)SettingUpView;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (Flowable)
            {
                _settingUpViews = Create(startOffset);
                for (int i = 0; i < SettingUpViews.Count; i++)
                    SettingUpViews[i].CompositeBinding.BeginSetup(_bindings);
            }
            else if (startOffset == 0)
            {
                SettingUpView = CreateView(0);
                SettingUpView.CompositeBinding.BeginSetup(_bindings);
            }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!Flowable);
            SettingUpView = value == null ? CreateView(0) : (ICompositeView)value;
            SettingUpView.CompositeBinding.BeginSetup(_bindings);
        }

        internal sealed override void PrepareSettingUpElement(int flowIndex)
        {
            if (Flowable)
            {
                Debug.Assert(SettingUpViews != null);
                SettingUpView = SettingUpViews[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (Flowable)
                SettingUpView = null;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            EnterSetup(flowIndex);

            var result = SettingUpView;
            for (int i = 0; i < _bindings.Count; i++)
                _bindings[i].Setup(flowIndex);

            ExitSetup();
            return (UIElement)result;
        }

        internal sealed override void Refresh(UIElement element)
        {
            ((ICompositeView)element).CompositeBinding.Refresh(_bindings);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var view = (ICompositeView)element;
            view.CompositeBinding.Cleanup(_bindings);
            element.SetScalarFlowIndex(0);
        }

        internal sealed override void EndSetup()
        {
            if (Flowable)
            {
                for (int i = 0; i < SettingUpViews.Count; i++)
                    SettingUpViews[i].CompositeBinding.EndSetup(_bindings);
            }
            else
                SettingUpView.CompositeBinding.EndSetup(_bindings);
            _settingUpViews = null;
            SettingUpView = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((ICompositeView)element).CompositeBinding.FlushInput(_bindings);
        }
    }
}
