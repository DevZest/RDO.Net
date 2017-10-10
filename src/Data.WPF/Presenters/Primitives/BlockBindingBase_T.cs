using DevZest.Data.Views;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class BlockBindingBase<T> : BlockBinding
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

        internal sealed override UIElement Setup(BlockView blockView)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetBlockView(blockView);
            PerformSetup(blockView);
            return SettingUpElement;
        }

        internal abstract void PerformSetup(BlockView blockView);

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }
    }
}
