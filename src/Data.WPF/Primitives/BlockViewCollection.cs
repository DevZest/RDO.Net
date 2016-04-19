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
            get { return ScrollableBlockCount == 0 ? null : (BlockView)Elements[BlockViewStartIndex + FixedHeadBlockCount]; }
        }

        public BlockView LastScrollable
        {
            get { return ScrollableBlockCount == 0 ? null : (BlockView)Elements[BlockViewStartIndex + FixedHeadBlockCount + ScrollableBlockCount - 1]; }
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

                var index = row.Ordinal / BlockDimensions;

                if (index < FixedHeadBlockCount)
                    return this[index];

                var fixedTailStartIndex = MaxBlockCount - FixedTailBlockCount;
                if (index >= fixedTailStartIndex)
                    return this[FixedHeadBlockCount + ScrollableBlockCount + (index - fixedTailStartIndex)];

                var firstScrollable = FirstScrollable;
                if (firstScrollable == null)
                    return null;

                if (index >= firstScrollable.Index && index <= LastScrollable.Index)
                    return this[FixedHeadBlockCount + (index - firstScrollable.Index)];
                return null;
            }
        }

        private int _fixedHeadBlockCount = -1;
        public int FixedHeadBlockCount
        {
            get
            {
                EnsureInitialized();
                return _fixedHeadBlockCount;
            }
        }

        private int _fixedTailBlockCount = -1;
        public int FixedTailBlockCount
        {
            get
            {
                EnsureInitialized();
                return _fixedTailBlockCount;
            }
        }

        public int ScrollableBlockCount { get; private set; }

        public void EnsureInitialized()
        {
            if (IsInitialized)
                return;

            _fixedHeadBlockCount = CoerceFixedHeadBlockCount();
            Debug.Assert(FixedHeadBlockCount >= 0);
            for (int i = 0; i < _fixedHeadBlockCount; i++)
            {
                var blockView = Realize(i);
                Insert(BlockViewStartIndex + i, blockView);
            }

            _fixedTailBlockCount = CoerceFixedTailBlockCount();
            Debug.Assert(FixedTailBlockCount >= 0);
            if (_fixedTailBlockCount == 0)
                return;

            var startIndex = MaxBlockCount - _fixedTailBlockCount;
            for (int i = 0; i < _fixedTailBlockCount; i++)
            {
                var blockView = Realize(i + startIndex);
                Insert(BlockViewStartIndex + _fixedHeadBlockCount + i, blockView);
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

        private int FixedHeadLastTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                return (Orientation == System.Windows.Controls.Orientation.Vertical ? Template.FixedTop : Template.FixedLeft) - 1;
            }
        }

        private int FixedTailFirstTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                var fixedTail = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.FixedBottom : Template.FixedRight;
                var lastTrackOrdinal = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.Range().Bottom.Ordinal : Template.Range().Right.Ordinal;
                return lastTrackOrdinal - fixedTail + 1;
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

        private int CoerceFixedHeadBlockCount()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var fixedHeadLastTrack = FixedHeadLastTrack;
            var rowRangeStartTrack = RowRangeStartTrack;
            if (fixedHeadLastTrack < rowRangeStartTrack)
                return 0;

            var repeatTracks = fixedHeadLastTrack - rowRangeStartTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var blockCount = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(blockCount, MaxBlockCount);
        }

        private int CoerceFixedTailBlockCount()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var fixedTailFirstTrack = FixedTailFirstTrack;
            var rowRangeEndTrack = RowRangeEndTrack;
            if (fixedTailFirstTrack > rowRangeEndTrack)
                return 0;

            var repeatTracks = rowRangeEndTrack - fixedTailFirstTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var blockCount = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(blockCount, Math.Max(0, MaxBlockCount - FixedHeadBlockCount));

        }

        public bool IsInitialized
        {
            get { return _fixedHeadBlockCount >= 0; }
        }

        public int Count
        {
            get { return FixedHeadBlockCount + ScrollableBlockCount + FixedTailBlockCount; }
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
            if (Template.BlockViewInitializer != null)
                Template.BlockViewInitializer(blockView);
        }

        internal void RealizeFirst(int index)
        {
            EnsureInitialized();
            Debug.Assert(ScrollableBlockCount == 0 && index >= FixedHeadBlockCount && index < MaxBlockCount - FixedTailBlockCount);

            var blockView = Realize(index);
            Insert(BlockViewStartIndex + FixedHeadBlockCount, blockView);
            ScrollableBlockCount = 1;
        }

        internal void RealizePrev()
        {
            Debug.Assert(FirstScrollable != null && FirstScrollable.Index - 1 >= FixedHeadBlockCount);

            var blockView = Realize(FirstScrollable.Index - 1);
            Insert(BlockViewStartIndex + FixedHeadBlockCount, blockView);
            ScrollableBlockCount++;
        }

        internal void RealizeNext()
        {
            Debug.Assert(LastScrollable != null && LastScrollable.Index + 1 < MaxBlockCount - FixedTailBlockCount);

            var blockView = Realize(LastScrollable.Index + 1);
            Insert(BlockViewStartIndex + FixedHeadBlockCount + ScrollableBlockCount, blockView);
            ScrollableBlockCount++;
        }

        internal void VirtualizeHead(int count)
        {
            Debug.Assert(count >= 0 && count <= ScrollableBlockCount);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Virtualize(this[FixedHeadBlockCount + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + FixedHeadBlockCount, count);
            ScrollableBlockCount -= count;
        }

        internal void VirtualizeTail(int count)
        {
            Debug.Assert(count >= 0 && count <= ScrollableBlockCount);

            if (count == 0)
                return;

            var offset = ScrollableBlockCount - count;
            for (int i = 0; i < count; i++)
                Virtualize(this[FixedHeadBlockCount + offset + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + FixedHeadBlockCount + offset, count);
            ScrollableBlockCount -= count;
        }

        internal void VirtualizeAll()
        {
            if (!IsInitialized)
                return;

            for (int i = 0; i < Count; i++)
                Virtualize(this[i]);
            ElementCollection.RemoveRange(BlockViewStartIndex, Count);

            _fixedHeadBlockCount = _fixedTailBlockCount = -1;
            ScrollableBlockCount = 0;
        }
    }
}
