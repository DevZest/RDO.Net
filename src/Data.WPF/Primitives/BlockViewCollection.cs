using DevZest.Data.Windows.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class BlockViewCollection : IReadOnlyList<BlockView>
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

        private BlockView Mount(int blockOrdinal)
        {
            var blockView = CachedList.GetOrCreate(ref _cachedBlockViews, Template.BlockViewConstructor);
            blockView.Initialize(_elementManager, blockOrdinal);
            return blockView;
        }

        private void Unmount(BlockView blockView)
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

        public bool Contains(RowPresenter row)
        {
            Debug.Assert(row != null && row.RowManager == _elementManager);
            return this[row] != null;
        }

        internal BlockView this[RowPresenter row]
        {
            get
            {
                if (Count == 0)
                    return null;

                var index = IndexOf(row.BlockOrdinal);
                return index == -1 ? null : this[index];
            }
        }

        internal int IndexOf(int ordinal)
        {
            Debug.Assert(ordinal >= 0 && ordinal < MaxBlockCount);

            var first = First;
            if (first == null)
                return -1;

            if (ordinal >= first.Ordinal && ordinal <= Last.Ordinal)
                return ordinal - first.Ordinal;
            return -1;
        }

        private GridRange RowRange
        {
            get { return Template.RowRange; }
        }

        private Orientation? Orientation
        {
            get { return Template.Orientation; }
        }

        public int MaxBlockCount
        {
            get { return Rows.Count == 0 ? 0 : (Rows.Count - 1) / BlockDimensions + 1; }
        }

        public int Count { get; private set; }

        public BlockView this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return (BlockView)Elements[BlockViewStartIndex + index];
            }
        }

        public IEnumerator<BlockView> GetEnumerator()
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
            Template.InitializeBlockView(blockView);
        }

        internal void MountFirst(int blockOrdinal)
        {
            Debug.Assert(Count == 0 && blockOrdinal >= 0 && blockOrdinal < MaxBlockCount);

            var blockView = Mount(blockOrdinal);
            Insert(BlockViewStartIndex, blockView);
            Count = 1;
        }

        internal void MountPrev()
        {
            Debug.Assert(First != null && First.Ordinal >= 1);

            var blockView = Mount(First.Ordinal - 1);
            Insert(BlockViewStartIndex, blockView);
            Count++;
        }

        internal void MountNext()
        {
            Debug.Assert(Last != null && Last.Ordinal + 1 < MaxBlockCount);

            var blockView = Mount(Last.Ordinal + 1);
            Insert(BlockViewStartIndex + Count, blockView);
            Count++;
        }

        internal void UnmountHead(int count)
        {
            Debug.Assert(count >= 0 && count <= Count);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Unmount(this[i]);

            ElementCollection.RemoveRange(BlockViewStartIndex, count);
            Count -= count;
        }

        internal void UnmountTail(int count)
        {
            Debug.Assert(count >= 0 && count <= Count);

            if (count == 0)
                return;

            var offset = Count - count;
            for (int i = 0; i < count; i++)
                Unmount(this[offset + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + offset, count);
            Count -= count;
        }

        internal void UnmountAll()
        {
            for (int i = 0; i < Count; i++)
                Unmount(this[i]);
            ElementCollection.RemoveRange(BlockViewStartIndex, Count);
            Count = 0;
        }

        internal void Clear()
        {
            for (int i = 0; i < Count; i++)
                Unmount(this[i]);
            ElementCollection.RemoveRange(BlockViewStartIndex, Count);
            Count = 0;
        }

        private bool IsMounted(int blockOrdinal)
        {
            return First != null && blockOrdinal >= First.Ordinal && blockOrdinal <= Last.Ordinal;
        }

        public BlockView GetBlockView(int blockOrdinal)
        {
            return IsMounted(blockOrdinal) ? this[blockOrdinal - First.Ordinal] : null;
        }

        public double AvgLength
        {
            get { return Count == 0 ? 1 : TotalLength / Count; }
        }

        private double TotalLength
        {
            get
            {
                Debug.Assert(Count > 0);
                return Last.EndOffset - First.StartOffset;
            }
        }
    }
}
