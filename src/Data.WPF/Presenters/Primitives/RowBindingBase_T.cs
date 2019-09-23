using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Base class for strongly typed row binding.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RowBindingBase<T> : RowBinding
        where T : UIElement, new()
    {
        /// <summary>
        /// Gets the view element that is currently being setup.
        /// </summary>
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

        /// <summary>
        /// Gets the view element for specified row.
        /// </summary>
        /// <param name="rowPresenter">The specified row.</param>
        /// <returns>The result view element.</returns>
        public new T this[RowPresenter rowPresenter]
        {
            get { return (T)base[rowPresenter]; }
        }

        /// <inheritdoc/>
        public sealed override Type ViewType
        {
            get { return typeof(T); }
        }
    }
}
