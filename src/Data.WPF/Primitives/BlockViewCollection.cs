using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
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

        public BlockView FirstScrollable
        {
            get { return ScrollableBlockCount == 0 ? null : (BlockView)Elements[BlockViewStartIndex + FrozenHeadBlockCount]; }
        }

        public BlockView LastScrollable
        {
            get { return ScrollableBlockCount == 0 ? null : (BlockView)Elements[BlockViewStartIndex + FrozenHeadBlockCount + ScrollableBlockCount - 1]; }
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

            EnsureInitialized();
            return this[row] != null;
        }

        internal BlockView this[RowPresenter row]
        {
            get
            {
                Debug.Assert(IsInitialized);

                if (Count == 0)
                    return null;

                var index = IndexOf(row.BlockOrdinal);
                return index == -1 ? null : this[index];
            }
        }

        internal int IndexOf(int ordinal)
        {
            Debug.Assert(ordinal >= 0 && ordinal < MaxBlockCount);

            if (ordinal < FrozenHeadBlockCount)
                return ordinal;

            var frozenTailStartOrdinal = MaxBlockCount - FrozenTailBlockCount;
            if (ordinal >= frozenTailStartOrdinal)
                return FrozenHeadBlockCount + ScrollableBlockCount + (ordinal - frozenTailStartOrdinal);

            var firstScrollable = FirstScrollable;
            if (firstScrollable == null)
                return -1;

            if (ordinal >= firstScrollable.Ordinal && ordinal <= LastScrollable.Ordinal)
                return FrozenHeadBlockCount + (ordinal - firstScrollable.Ordinal);
            return -1;
        }

        private int _frozenHeadBlockCount = -1;
        public int FrozenHeadBlockCount
        {
            get
            {
                EnsureInitialized();
                return _frozenHeadBlockCount;
            }
        }

        private int _frozenTailBlockCount = -1;
        public int FrozenTailBlockCount
        {
            get
            {
                EnsureInitialized();
                return _frozenTailBlockCount;
            }
        }

        public int ScrollableBlockCount { get; private set; }

        public void EnsureInitialized()
        {
            if (IsInitialized)
                return;

            _frozenHeadBlockCount = CoerceFrozenHeadBlockCount();
            Debug.Assert(FrozenHeadBlockCount >= 0);
            for (int i = 0; i < _frozenHeadBlockCount; i++)
            {
                var blockView = Realize(i);
                Insert(BlockViewStartIndex + i, blockView);
            }

            _frozenTailBlockCount = CoerceFrozenTailBlockCount();
            Debug.Assert(FrozenTailBlockCount >= 0);
            if (_frozenTailBlockCount == 0)
                return;

            var startIndex = MaxBlockCount - _frozenTailBlockCount;
            for (int i = 0; i < _frozenTailBlockCount; i++)
            {
                var blockView = Realize(i + startIndex);
                Insert(BlockViewStartIndex + _frozenHeadBlockCount + i, blockView);
            }
        }

        private GridRange RowRange
        {
            get { return Template.RowRange; }
        }

        private Orientation? Orientation
        {
            get { return Template.Orientation; }
        }

        private int FrozenHeadLastTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                return (Orientation == System.Windows.Controls.Orientation.Vertical ? Template.FrozenTop : Template.FrozenLeft) - 1;
            }
        }

        private int FrozenTailFirstTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                var frozenTail = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.FrozenBottom : Template.FrozenRight;
                var lastTrackOrdinal = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.Range().Bottom.Ordinal : Template.Range().Right.Ordinal;
                return lastTrackOrdinal - frozenTail + 1;
            }
        }

        private int RowRangeStartTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                return Orientation == System.Windows.Controls.Orientation.Vertical ? RowRange.Top.Ordinal : RowRange.Left.Ordinal;
            }
        }

        private int RowRangeEndTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                return Orientation == System.Windows.Controls.Orientation.Vertical ? RowRange.Bottom.Ordinal : RowRange.Right.Ordinal;
            }
        }

        private int RowRangeTracks
        {
            get { return RowRangeEndTrack - RowRangeStartTrack + 1; }
        }

        private int MaxBlockCount
        {
            get { return Rows.Count == 0 ? 0 : (Rows.Count - 1) / BlockDimensions + 1; }
        }

        private int CoerceFrozenHeadBlockCount()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var frozenHeadLastTrack = FrozenHeadLastTrack;
            var rowRangeStartTrack = RowRangeStartTrack;
            if (frozenHeadLastTrack < rowRangeStartTrack)
                return 0;

            var repeatTracks = frozenHeadLastTrack - rowRangeStartTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var blockCount = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(blockCount, MaxBlockCount);
        }

        private int CoerceFrozenTailBlockCount()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var frozenTailFirstTrack = FrozenTailFirstTrack;
            var rowRangeEndTrack = RowRangeEndTrack;
            if (frozenTailFirstTrack > rowRangeEndTrack)
                return 0;

            var repeatTracks = rowRangeEndTrack - frozenTailFirstTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var blockCount = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(blockCount, Math.Max(0, MaxBlockCount - FrozenHeadBlockCount));

        }

        public bool IsInitialized
        {
            get { return _frozenHeadBlockCount >= 0; }
        }

        public int Count
        {
            get { return FrozenHeadBlockCount + ScrollableBlockCount + FrozenTailBlockCount; }
        }

        public BlockView this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
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
            Template.InitializeBlockView(blockView);
        }

        internal void RealizeFirst(int index)
        {
            EnsureInitialized();
            Debug.Assert(ScrollableBlockCount == 0 && index >= FrozenHeadBlockCount && index < MaxBlockCount - FrozenTailBlockCount);

            var blockView = Realize(index);
            Insert(BlockViewStartIndex + FrozenHeadBlockCount, blockView);
            ScrollableBlockCount = 1;
        }

        internal void RealizePrev()
        {
            Debug.Assert(FirstScrollable != null && FirstScrollable.Ordinal - 1 >= FrozenHeadBlockCount);

            var blockView = Realize(FirstScrollable.Ordinal - 1);
            Insert(BlockViewStartIndex + FrozenHeadBlockCount, blockView);
            ScrollableBlockCount++;
        }

        internal void RealizeNext()
        {
            Debug.Assert(LastScrollable != null && LastScrollable.Ordinal + 1 < MaxBlockCount - FrozenTailBlockCount);

            var blockView = Realize(LastScrollable.Ordinal + 1);
            Insert(BlockViewStartIndex + FrozenHeadBlockCount + ScrollableBlockCount, blockView);
            ScrollableBlockCount++;
        }

        internal void VirtualizeHead(int count)
        {
            Debug.Assert(count >= 0 && count <= ScrollableBlockCount);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Virtualize(this[FrozenHeadBlockCount + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + FrozenHeadBlockCount, count);
            ScrollableBlockCount -= count;
        }

        internal void VirtualizeTail(int count)
        {
            Debug.Assert(count >= 0 && count <= ScrollableBlockCount);

            if (count == 0)
                return;

            var offset = ScrollableBlockCount - count;
            for (int i = 0; i < count; i++)
                Virtualize(this[FrozenHeadBlockCount + offset + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + FrozenHeadBlockCount + offset, count);
            ScrollableBlockCount -= count;
        }

        internal void VirtualizeAll()
        {
            if (!IsInitialized)
                return;

            for (int i = 0; i < Count; i++)
                Virtualize(this[i]);
            ElementCollection.RemoveRange(BlockViewStartIndex, Count);

            _frozenHeadBlockCount = _frozenTailBlockCount = -1;
            ScrollableBlockCount = 0;
        }
    }
}
