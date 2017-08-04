﻿using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeScalarBinding : ScalarBinding, ICompositeBinding
    {
        private List<ScalarBinding> _bindings = new List<ScalarBinding>();
        private List<string> _names = new List<string>();

        IReadOnlyList<Binding> ICompositeBinding.Bindings
        {
            get { return _bindings; }
        }

        IReadOnlyList<string> ICompositeBinding.Names
        {
            get { return _names; }
        }

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

            if (startOffset == FlowRepeatCount)
                return Array<ICompositeView>.Empty;

            var count = FlowRepeatCount - startOffset;
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
            result.BindingDispatcher.Initialize(this);
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
            Debug.Assert(!FlowRepeatable);
            return (UIElement)SettingUpView;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (FlowRepeatable)
            {
                _settingUpViews = Create(startOffset);
                for (int i = 0; i < SettingUpViews.Count; i++)
                    SettingUpViews[i].BindingDispatcher.BeginSetup();
            }
            else if (startOffset == 0)
            {
                SettingUpView = CreateView(0);
                SettingUpView.BindingDispatcher.BeginSetup();
            }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!FlowRepeatable);
            SettingUpView = value == null ? CreateView(0) : (ICompositeView)value;
            SettingUpView.BindingDispatcher.BeginSetup();
        }

        internal sealed override void PrepareSettingUpElement(int flowIndex)
        {
            if (FlowRepeatable)
            {
                Debug.Assert(SettingUpViews != null);
                SettingUpView = SettingUpViews[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (FlowRepeatable)
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

        private bool _isRefreshing;
        internal sealed override bool IsRefreshing
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
            element.SetScalarFlowIndex(0);
        }

        internal sealed override void EndSetup()
        {
            if (FlowRepeatable)
            {
                for (int i = 0; i < SettingUpViews.Count; i++)
                    SettingUpViews[i].BindingDispatcher.EndSetup();
            }
            else
                SettingUpView.BindingDispatcher.EndSetup();
            _settingUpViews = null;
            SettingUpView = null;
        }

        internal sealed override void FlushInput(UIElement element)
        {
            ((ICompositeView)element).BindingDispatcher.FlushInput();
        }
    }
}
