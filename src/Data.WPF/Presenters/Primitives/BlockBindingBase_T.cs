using DevZest.Data.Views;
using System.Diagnostics;
using System.Windows;
using System;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Base class for strongly type block binding.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BlockBindingBase<T> : BlockBinding
        where T : UIElement, new()
    {
        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        /// <summary>
        /// Gets the view element that is setting up.
        /// </summary>
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

        internal sealed override UIElement Setup(BlockView blockView)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetBlockView(blockView);
            PerformSetup(blockView);
            return SettingUpElement;
        }

        internal abstract void PerformSetup(BlockView blockView);

        /// <summary>
        /// Gets the view element at specified block ordinal.
        /// </summary>
        /// <param name="blockOrdinal">The specified block ordinal.</param>
        /// <returns>The view element.</returns>
        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }

        /// <inheritdoc/>
        public sealed override Type ViewType
        {
            get { return typeof(T); }
        }
    }
}
