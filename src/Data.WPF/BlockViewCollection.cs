using DevZest.Data.Windows.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal sealed class BlockViewCollection : IReadOnlyList<IBlockPresenter>
    {
        internal BlockViewCollection(ElementManager elementManager)
        {
            Debug.Assert(elementManager != null);
            _elementManager = elementManager;
        }

        private readonly ElementManager _elementManager;

        private Template Template
        {
            get { return _elementManager.Template; }
        }

        private IElementCollection ElementCollection
        {
            get { return _elementManager.ElementCollection; }
        }

        private IReadOnlyList<UIElement> Elements
        {
            get { return _elementManager.Elements; }
        }

        private int BlockViewStartIndex
        {
            get { return _elementManager.BlockViewStartIndex; }
        }

        List<BlockView> _cachedBlockViews;

        private BlockView Realize(int index)
        {
            var blockView = CachedList.GetOrCreate(ref _cachedBlockViews, Template.BlockViewConstructor);
            blockView.Initialize(_elementManager, index);
            return blockView;
        }

        private void Virtualize(BlockView blockView)
        {
            Debug.Assert(blockView != null);

            if (Template.BlockViewCleanupAction != null)
                Template.BlockViewCleanupAction(blockView);
            blockView.Cleanup();
            CachedList.Recycle(ref _cachedBlockViews, blockView);
        }

        public BlockView First
        {
            get { return Count == 0 ? null : (BlockView)Elements[BlockViewStartIndex]; }
        }

        public BlockView Last
        {
            get { return Count == 0 ? null : (BlockView)Elements[BlockViewStartIndex + Count - 1]; }
        }

        private IReadOnlyList<RowPresenter> Rows
        {
            get { return _elementManager.Rows; }
        }

        private int BlockDimensions
        {
            get { return _elementManager.BlockDimensions; }
        }

        public RowPresenter FirstRow
        {
            get { return Count == 0 ? null : Rows[First.Index * BlockDimensions]; }
        }

        public RowPresenter LastRow
        {
            get { return Count == 0 ? null : Rows[Math.Min(Rows.Count - 1, (Last.Index + 1) * BlockDimensions - 1)]; }
        }

        public bool Contains(RowPresenter row)
        {
            Debug.Assert(row != null && row.RowManager == _elementManager);
            if (Count == 0)
                return false;
            return row.Ordinal >= FirstRow.Ordinal && row.Ordinal <= LastRow.Ordinal;
        }

        public int Count { get; private set; }

        public BlockView this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < Count);
                return (BlockView)Elements[BlockViewStartIndex + index];
            }
        }

        IBlockPresenter IReadOnlyList<IBlockPresenter>.this[int index]
        {
            get { return this[index]; }
        }

        public IEnumerator<IBlockPresenter> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Insert(int index, BlockView blockView)
        {
            ElementCollection.Insert(index, blockView);
            if (Template.BlockViewInitializer != null)
                Template.BlockViewInitializer(blockView);
        }

        internal void RealizeFirst(int index)
        {
            Debug.Assert(Count == 0 && index >= 0 && index * BlockDimensions < Rows.Count);

            var blockView = Realize(index);
            Insert(BlockViewStartIndex, blockView);
            Count = 1;
        }

        internal void RealizePrev()
        {
            Debug.Assert(First != null && First.Index > 0);

            var blockView = Realize(First.Index - 1);
            Insert(BlockViewStartIndex, blockView);
            Count++;
        }

        internal void RealizeNext()
        {
            Debug.Assert(LastRow != null && LastRow.Ordinal < Rows.Count - 1);

            var blockView = Realize(Last.Index + 1);
            Insert(BlockViewStartIndex + Count, blockView);
            Count++;
        }

        internal void VirtualizeHead(int count)
        {
            Debug.Assert(count >= 0 && count <= Count);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Virtualize(this[i]);

            ElementCollection.RemoveRange(BlockViewStartIndex, count);
            Count -= count;
        }

        internal void VirtualizeTail(int count)
        {
            Debug.Assert(count >= 0 && count <= Count);

            if (count == 0)
                return;

            var offset = Count - count;
            for (int i = 0; i < count; i++)
                Virtualize(this[offset + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + offset, count);
            Count -= count;
        }

        internal void VirtualizeAll()
        {
            VirtualizeHead(Count);
        }
    }
}
