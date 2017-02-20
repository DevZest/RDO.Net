using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class BlockBinding<T> : BlockBinding
        where T : UIElement, new()
    {
        public BlockBinding(Action<T, IBlockPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public BlockBinding(Action<T, IBlockPresenter> onRefresh,
            Action<T, IBlockPresenter> onSetup,
            Action<T, IBlockPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

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

        internal sealed override void BeginSetup(UIElement value)
        {
            SettingUpElement = value == null ? Create() : (T)value;
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetBlockView(blockView);
            Setup(SettingUpElement, blockView);
            Refresh(SettingUpElement, blockView);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        private Action<T, IBlockPresenter> _onSetup;
        private void Setup(T element, IBlockPresenter blockPresenter)
        {
            if (_onSetup != null)
                _onSetup(element, blockPresenter);
        }

        private Action<T, IBlockPresenter> _onRefresh;
        private void Refresh(T element, IBlockPresenter blockPresenter)
        {
            if (_onRefresh != null)
                _onRefresh(element, blockPresenter);
        }

        private Action<T, IBlockPresenter> _onCleanup;
        private void Cleanup(T element, IBlockPresenter blockPresenter)
        {
            if (_onCleanup != null)
                _onCleanup(element, blockPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            var blockPresenter = element.GetBlockView();
            Refresh((T)element, blockPresenter);
        }

        internal override void Cleanup(UIElement element)
        {
            var blockPresenter = element.GetBlockView();
            var e = (T)element;
            Cleanup(e, blockPresenter);
            e.SetBlockView(null);
        }

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }
    }
}
