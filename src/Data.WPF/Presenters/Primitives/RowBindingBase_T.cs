using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal override void BeginSetup(UIElement value)
        {
            SettingUpElement = value == null ? new T() : (T)value;
            if (SettingUpElement.GetBinding() == null)
                OnCreated(SettingUpElement);
        }

        internal override void EndSetup()
        {
            SettingUpElement = null;
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetRowPresenter(this, rowPresenter);
            PerformSetup(rowPresenter);
            return SettingUpElement;
        }

        internal sealed override void Cleanup(UIElement element)
        {
            PerformCleanup(element);
            element.SetRowPresenter(this, null);
        }

        internal abstract void PerformSetup(RowPresenter rowPresenter);

        internal abstract void PerformCleanup(UIElement element);

        public new T this[RowPresenter rowPresenter]
        {
            get { return (T)base[rowPresenter]; }
        }

        public sealed override Type ViewType
        {
            get { return typeof(T); }
        }
    }
}
