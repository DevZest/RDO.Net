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
            _settingUpStartOffset = startOffset;

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
            if (Parent != null)
                result.SetScalarFlowIndex(ScalarPresenter.FlowIndex);
            return result;
        }

        private int _settingUpStartOffset;
        private T[] _settingUpElements;
        public IReadOnlyList<T> SettingUpElements
        {
            get { return _settingUpElements; }
        }

        public T SettingUpElement { get; private set; }

        internal override void BeginSetup(int startOffset, UIElement[] elements)
        {
            Debug.Assert(FlowRepeatable);
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

        internal sealed override void PerformEnterSetup(int flowIndex)
        {
            Debug.Assert(FlowRepeatable);
            SettingUpElement = SettingUpElements[flowIndex - _settingUpStartOffset];
        }

        internal sealed override void PerformExitSetup()
        {
            Debug.Assert(FlowRepeatable);
            SettingUpElement = null;
        }

        internal sealed override void EndSetup()
        {
            _settingUpElements = null;
            SettingUpElement = null;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            EnterSetup(flowIndex);
            var result = SettingUpElement;
            PerformSetup(ScalarPresenter);
            ExitSetup();
            return result;
        }

        internal abstract void PerformSetup(ScalarPresenter scalarPresenter);

        private void EnterSetup(int flowIndex)
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                if (scalarBinding.FlowRepeatable)
                    scalarBinding.PerformEnterSetup(flowIndex);
            }

            ScalarPresenter.SetFlowIndex(flowIndex);
        }

        private void ExitSetup()
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
            {
                var scalarBinding = scalarBindings[i];
                if (scalarBinding.FlowRepeatable)
                    scalarBinding.PerformExitSetup();
            }

            ScalarPresenter.SetFlowIndex(0);
        }
    }
}
