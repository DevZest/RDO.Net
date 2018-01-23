using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }

        private T[] Create(int startOffset)
        {
            if (startOffset == FlowRepeatCount)
                return Array<T>.Empty;

            int count = FlowRepeatCount - startOffset;
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Create();
                result[i].SetScalarFlowIndex(startOffset + i);
            }
            return result;
        }

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        private int _settingUpStartOffset;
        private T[] _settingUpElements;
        internal IReadOnlyList<T> SettingUpElements
        {
            get { return _settingUpElements; }
        }

        private T SettingUpElement { get; set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal override void BeginSetup(int startOffset, UIElement[] elements)
        {
            Debug.Assert(FlowRepeatable);
            _settingUpStartOffset = startOffset;
            _settingUpElements = elements == null ? Create(startOffset) : Cast(elements);
        }

        private static T[] Cast(UIElement[] elements)
        {
            var result = new T[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                result[i] = (T)elements[i];
            return result;
        }

        internal override void BeginSetup(UIElement element)
        {
            SettingUpElement = element == null ? Create() : (T)element;
        }

        public T GetSettingUpElement(int flowIndex)
        {
            if (FlowRepeatable)
            {
                if (SettingUpElements == null)
                    return null;
                var index = flowIndex - _settingUpStartOffset;
                if (index < 0 || index >= SettingUpElements.Count)
                    return null;
                return SettingUpElements[index];
            }
            else
                return SettingUpElement;
        }

        public T GetSettingUpElement(ScalarPresenter scalarPresenter)
        {
            if (scalarPresenter == null)
                throw new ArgumentNullException(nameof(scalarPresenter));
            return GetSettingUpElement(scalarPresenter.FlowIndex);
        }

        internal override void EndSetup()
        {
            _settingUpElements = null;
            SettingUpElement = null;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            var result = GetSettingUpElement(flowIndex);
            ScalarPresenter.EnterSetup(flowIndex);
            PerformSetup(result, ScalarPresenter);
            ScalarPresenter.ExitSetup();
            return result;
        }

        internal abstract void PerformSetup(T element, ScalarPresenter scalarPresenter);

        public sealed override Type ViewType
        {
            get { return typeof(T); }
        }
    }
}
