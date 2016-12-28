using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class BlockBinding<T> : BlockBinding
        where T : UIElement, new()
    {
        List<T> _cachedElements;

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

        internal sealed override void SetSettingUpElement(UIElement value)
        {
            SettingUpElement = (T)value;
        }

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetBlockView(blockView);
            Setup(SettingUpElement, blockView.Ordinal, blockView);
            Refresh(SettingUpElement, blockView.Ordinal, blockView);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        protected virtual void Setup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
        }

        protected abstract void Refresh(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows);

        protected virtual void Cleanup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
        }

        internal sealed override void Refresh(UIElement element)
        {
            var blockView = element.GetBlockView();
            Refresh((T)element, blockView.Ordinal, blockView);
        }

        internal override void Cleanup(UIElement element, bool recycle)
        {
            var blockView = element.GetBlockView();
            var e = (T)element;
            Cleanup(e, blockView.Ordinal, blockView);
            e.SetBlockView(null);
            if (recycle)
                CachedList.Recycle(ref _cachedElements, e);
        }
    }
}
