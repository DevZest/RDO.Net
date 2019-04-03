using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    public sealed class BlockBinding<T> : BlockBindingBase<T>
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

        private BlockPresenter BlockPresenter
        {
            get { return Template.BlockPresenter; }
        }

        internal sealed override void PerformSetup(BlockView blockView)
        {
            BlockPresenter.BlockView = blockView;
            Setup(SettingUpElement, BlockPresenter);
            Refresh(SettingUpElement, BlockPresenter);
            BlockPresenter.BlockView = null;
        }

        private Action<T, BlockPresenter> _onSetup;
        private void Setup(T element, BlockPresenter blockPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Setup(element, blockPresenter);
            if (_onSetup != null)
                _onSetup(element, blockPresenter);
            var blockElement = element as IBlockElement;
            if (blockElement != null)
                blockElement.Setup(blockPresenter);
        }

        private Action<T, BlockPresenter> _onRefresh;
        private void Refresh(T element, BlockPresenter blockPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Refresh(element, blockPresenter);
            if (_onRefresh != null)
                _onRefresh(element, blockPresenter);
            var blockElement = element as IBlockElement;
            if (blockElement != null)
                blockElement.Refresh(blockPresenter);
        }

        private Action<T, BlockPresenter> _onCleanup;
        private void Cleanup(T element, BlockPresenter blockPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Cleanup(element, blockPresenter);
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

        public BlockBinding<T> OverrideSetup(Action<T, BlockPresenter> overrideSetup)
        {
            if (overrideSetup == null)
                throw new ArgumentNullException(nameof(overrideSetup));
            _onSetup = _onSetup.Override(overrideSetup);
            return this;
        }

        public BlockBinding<T> OverrideRefresh(Action<T, BlockPresenter> overrideRefresh)
        {
            if (overrideRefresh == null)
                throw new ArgumentNullException(nameof(overrideRefresh));
            _onRefresh = _onRefresh.Override(overrideRefresh);
            return this;
        }

        public BlockBinding<T> OverrideCleanup(Action<T, BlockPresenter> overrideCleanup)
        {
            if (overrideCleanup == null)
                throw new ArgumentNullException(nameof(overrideCleanup));
            _onCleanup = _onRefresh.Override(overrideCleanup);
            return this;
        }

        private List<IBlockBindingBehavior<T>> _behaviors;
        public IReadOnlyList<IBlockBindingBehavior<T>> Behaviors
        {
            get
            {
                if (_behaviors == null)
                    return Array.Empty<IBlockBindingBehavior<T>>();
                else
                    return _behaviors;
            }
        }

        internal void InternalAddBehavior(IBlockBindingBehavior<T> behavior)
        {
            Debug.Assert(behavior != null);
            if (_behaviors == null)
                _behaviors = new List<IBlockBindingBehavior<T>>();
            _behaviors.Add(behavior);
        }

        internal override UIElement GetChild(UIElement parent, int index)
        {
            var containerElement = parent as IContainerElement;
            if (containerElement != null)
                return containerElement.GetChild(index);
            throw new NotSupportedException();
        }
    }
}
