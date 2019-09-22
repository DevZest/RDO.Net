using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents block level data binding.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public sealed class BlockBinding<T> : BlockBindingBase<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BlockBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">Delegate to refresh the binding.</param>
        public BlockBinding(Action<T, BlockPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">Delegate to refresh the binding.</param>
        /// <param name="onSetup">Delegate to setup the binding.</param>
        /// <param name="onCleanup">Delegate to cleanup the binding.</param>
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

        /// <summary>
        /// Applies additional setup logic.
        /// </summary>
        /// <param name="setup">Delegate to setup the binding.</param>
        /// <returns></returns>
        public BlockBinding<T> ApplySetup(Action<T, BlockPresenter> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            _onSetup = _onSetup.Apply(setup);
            return this;
        }

        /// <summary>
        /// Applies additional refresh logic.
        /// </summary>
        /// <param name="refresh">Delegate to refresh the binding.</param>
        /// <returns></returns>
        public BlockBinding<T> ApplyRefresh(Action<T, BlockPresenter> refresh)
        {
            if (refresh == null)
                throw new ArgumentNullException(nameof(refresh));
            _onRefresh = _onRefresh.Apply(refresh);
            return this;
        }

        /// <summary>
        /// Applies additional cleanup logic
        /// </summary>
        /// <param name="cleanup"></param>
        /// <returns></returns>
        public BlockBinding<T> ApplyCleanup(Action<T, BlockPresenter> cleanup)
        {
            if (cleanup == null)
                throw new ArgumentNullException(nameof(cleanup));
            _onCleanup = _onCleanup.Apply(cleanup);
            return this;
        }

        private List<IBlockBindingBehavior<T>> _behaviors;
        /// <summary>
        /// Gets the behaviors added into this binding.
        /// </summary>
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
