using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
using DevZest.Data.Presenters.Plugins;

namespace DevZest.Data.Presenters
{
    public sealed class BlockBinding<T> : BlockBinding
        where T : UIElement, new()
    {
        public BlockBinding(Action<T, BlockPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public BlockBinding(Action<T, BlockPresenter> onRefresh,
            Action<T, BlockPresenter> onSetup,
            Action<T, BlockPresenter> onCleanup)
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

        private BlockPresenter BlockPresenter
        {
            get { return Template.BlockPresenter; }
        }

        internal sealed override UIElement Setup(BlockView blockView)
        {
            Debug.Assert(SettingUpElement != null);
            SettingUpElement.SetBlockView(blockView);
            BlockPresenter.BlockView = blockView;
            Setup(SettingUpElement, BlockPresenter);
            Refresh(SettingUpElement, BlockPresenter);
            BlockPresenter.BlockView = null;
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        private Action<T, BlockPresenter> _onSetup;
        private void Setup(T element, BlockPresenter blockPresenter)
        {
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Setup(element, blockPresenter);
            if (_onSetup != null)
                _onSetup(element, blockPresenter);
            var blockElement = element as IBlockElement;
            if (blockElement != null)
                blockElement.Setup(blockPresenter);
        }

        private Action<T, BlockPresenter> _onRefresh;
        private void Refresh(T element, BlockPresenter blockPresenter)
        {
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Refresh(element, blockPresenter);
            if (_onRefresh != null)
                _onRefresh(element, blockPresenter);
            var blockElement = element as IBlockElement;
            if (blockElement != null)
                blockElement.Refresh(blockPresenter);
        }

        private Action<T, BlockPresenter> _onCleanup;
        private void Cleanup(T element, BlockPresenter blockPresenter)
        {
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Cleanup(element, blockPresenter);
            var blockElement = element as IBlockElement;
            if (blockElement != null)
                blockElement.Cleanup(blockPresenter);
            if (_onCleanup != null)
                _onCleanup(element, blockPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            BlockPresenter.BlockView = element.GetBlockView();
            Refresh((T)element, BlockPresenter);
            BlockPresenter.BlockView = null;
        }

        internal override void Cleanup(UIElement element)
        {
            BlockPresenter.BlockView = element.GetBlockView();
            Cleanup((T)element, BlockPresenter);
            BlockPresenter.BlockView = null;
            element.SetBlockView(null);
        }

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }

        public BlockBinding<T> OverrideSetup(Action<T, BlockPresenter, Action<T, BlockPresenter>> overrideSetup)
        {
            if (overrideSetup == null)
                throw new ArgumentNullException(nameof(overrideSetup));
            _onSetup = _onSetup.Override(overrideSetup);
            return this;
        }

        public BlockBinding<T> OverrideRefresh(Action<T, BlockPresenter, Action<T, BlockPresenter>> overrideRefresh)
        {
            if (overrideRefresh == null)
                throw new ArgumentNullException(nameof(overrideRefresh));
            _onRefresh = _onRefresh.Override(overrideRefresh);
            return this;
        }

        public BlockBinding<T> OverrideCleanup(Action<T, BlockPresenter, Action<T, BlockPresenter>> overrideCleanup)
        {
            if (overrideCleanup == null)
                throw new ArgumentNullException(nameof(overrideCleanup));
            _onCleanup = _onRefresh.Override(overrideCleanup);
            return this;
        }

        private List<IBlockBindingPlugin> _plugins;
        public IReadOnlyList<IBlockBindingPlugin> Plugins
        {
            get
            {
                if (_plugins == null)
                    return Array<IBlockBindingPlugin>.Empty;
                else
                    return _plugins;
            }
        }

        internal void InternalAddPlugin(IBlockBindingPlugin plugin)
        {
            Debug.Assert(plugin != null);
            if (_plugins == null)
                _plugins = new List<IBlockBindingPlugin>();
            _plugins.Add(plugin);
        }
    }
}
