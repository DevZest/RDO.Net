using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal override void BeginSetup(UIElement value)
        {
            SettingUpElement = value == null ? Create() : (T)value;
        }

        internal override void EndSetup()
        {
            SettingUpElement = null;
        }

        internal sealed override UIElement Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetRowPresenter(rowPresenter);
            PerformSetup(rowPresenter);
            return SettingUpElement;
        }

        internal abstract void PerformSetup(RowPresenter rowPresenter);

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
