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
        public BlockBinding(Action<T, int, IReadOnlyList<RowPresenter>> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public BlockBinding(Action<T, int, IReadOnlyList<RowPresenter>> onRefresh,
            Action<T, int, IReadOnlyList<RowPresenter>> onSetup,
            Action<T, int, IReadOnlyList<RowPresenter>> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

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

        internal sealed override void BeginSetup(UIElement value)
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
            Setup(SettingUpElement, blockView.ContainerOrdinal, blockView);
            Refresh(SettingUpElement, blockView.ContainerOrdinal, blockView);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        private Action<T, int, IReadOnlyList<RowPresenter>> _onSetup;
        private void Setup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (_onSetup != null)
                _onSetup(element, blockOrdinal, rows);
        }

        private Action<T, int, IReadOnlyList<RowPresenter>> _onRefresh;
        private void Refresh(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (_onRefresh != null)
                _onRefresh(element, blockOrdinal, rows);
        }

        private Action<T, int, IReadOnlyList<RowPresenter>> _onCleanup;
        private void Cleanup(T element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
        {
            if (_onCleanup != null)
                _onCleanup(element, blockOrdinal, rows);
        }

        internal sealed override void Refresh(UIElement element)
        {
            var blockView = element.GetBlockView();
            Refresh((T)element, blockView.ContainerOrdinal, blockView);
        }

        internal override void Cleanup(UIElement element, bool recycle)
        {
            var blockView = element.GetBlockView();
            var e = (T)element;
            Cleanup(e, blockView.ContainerOrdinal, blockView);
            e.SetBlockView(null);
            if (recycle)
                CachedList.Recycle(ref _cachedElements, e);
        }

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }
    }
}
