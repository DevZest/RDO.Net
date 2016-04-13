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

        public BlockView FirstUnpinned
        {
            get { return CountUnpinned == 0 ? null : (BlockView)Elements[BlockViewStartIndex + CountPinnedHead]; }
        }

        public BlockView LastUnpinned
        {
            get { return CountUnpinned == 0 ? null : (BlockView)Elements[BlockViewStartIndex + CountPinnedHead + CountUnpinned - 1]; }
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
            if (Count == 0)
                return false;

            var index = row.Ordinal / BlockDimensions;
            if (index < CountPinnedHead)
                return true;
            if (index >= MaxBlockCount - CountPinnedTail)
                return true;
            return FirstUnpinned == null ? false : index >= FirstUnpinned.Index && index <= LastUnpinned.Index;
        }

        private int _countPinnedHead = -1;
        public int CountPinnedHead
        {
            get
            {
                EnsureInitialized();
                return _countPinnedHead;
            }
        }

        private int _countPinnedTail = -1;
        public int CountPinnedTail
        {
            get
            {
                EnsureInitialized();
                return _countPinnedTail;
            }
        }

        public int CountUnpinned { get; private set; }

        public void EnsureInitialized()
        {
            if (IsInitialized)
                return;

            _countPinnedHead = CoerceCountPinnedHead();
            Debug.Assert(CountPinnedHead >= 0);
            for (int i = 0; i < _countPinnedHead; i++)
            {
                var blockView = Realize(i);
                Insert(BlockViewStartIndex + i, blockView);
            }

            _countPinnedTail = CoerceCountPinnedTail();
            Debug.Assert(CountPinnedTail >= 0);
            if (_countPinnedTail == 0)
                return;

            var startIndex = MaxBlockCount - _countPinnedTail;
            for (int i = 0; i < _countPinnedTail; i++)
            {
                var blockView = Realize(i + startIndex);
                Insert(BlockViewStartIndex + _countPinnedHead + i, blockView);
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

        private int PinnedHeadLastTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                return (Orientation == System.Windows.Controls.Orientation.Vertical ? Template.PinnedTop : Template.PinnedLeft) - 1;
            }
        }

        private int PinnedTailFirstTrack
        {
            get
            {
                Debug.Assert(Orientation.HasValue);
                var pinnedTail = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.PinnedBottom : Template.PinnedRight;
                var lastTrackOrdinal = Orientation == System.Windows.Controls.Orientation.Vertical ? Template.Range().Bottom.Ordinal : Template.Range().Right.Ordinal;
                return lastTrackOrdinal - pinnedTail + 1;
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

        private int CoerceCountPinnedHead()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var pinnedHeadLastTrack = PinnedHeadLastTrack;
            var rowRangeStartTrack = RowRangeStartTrack;
            if (pinnedHeadLastTrack < rowRangeStartTrack)
                return 0;

            var repeatTracks = pinnedHeadLastTrack - rowRangeStartTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var repeat = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(repeat, MaxBlockCount);
        }

        private int CoerceCountPinnedTail()
        {
            if (!Template.Orientation.HasValue)
                return 0;

            var pinnedTailFirstTrack = PinnedTailFirstTrack;
            var rowRangeEndTrack = RowRangeEndTrack;
            if (pinnedTailFirstTrack > rowRangeEndTrack)
                return 0;

            var repeatTracks = rowRangeEndTrack - pinnedTailFirstTrack + 1;
            var rowRangeTracks = RowRangeTracks;
            var repeat = ((repeatTracks - 1) / rowRangeTracks) + 1;
            return Math.Min(repeat, Math.Max(0, MaxBlockCount - CountPinnedHead));

        }

        public bool IsInitialized
        {
            get { return _countPinnedHead >= 0; }
        }

        public int Count
        {
            get { return CountPinnedHead + CountUnpinned + CountPinnedTail; }
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

        internal void RealizeFirstUnpinned(int index)
        {
            EnsureInitialized();
            Debug.Assert(CountUnpinned == 0 && index >= CountPinnedHead && index < MaxBlockCount - CountPinnedTail);

            var blockView = Realize(index);
            Insert(BlockViewStartIndex + CountPinnedHead, blockView);
            CountUnpinned = 1;
        }

        internal void RealizePrevUnpinned()
        {
            Debug.Assert(FirstUnpinned != null && FirstUnpinned.Index - 1 >= CountPinnedHead);

            var blockView = Realize(FirstUnpinned.Index - 1);
            Insert(BlockViewStartIndex + CountPinnedHead, blockView);
            CountUnpinned++;
        }

        internal void RealizeNextUnpinned()
        {
            Debug.Assert(LastUnpinned != null && LastUnpinned.Index + 1 < MaxBlockCount - CountPinnedTail);

            var blockView = Realize(LastUnpinned.Index + 1);
            Insert(BlockViewStartIndex + CountPinnedHead + CountUnpinned, blockView);
            CountUnpinned++;
        }

        internal void VirtualizeUnpinnedHead(int count)
        {
            Debug.Assert(count >= 0 && count <= CountUnpinned);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Virtualize(this[CountPinnedHead + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + CountPinnedHead, count);
            CountUnpinned -= count;
        }

        internal void VirtualizeUnpinnedTail(int count)
        {
            Debug.Assert(count >= 0 && count <= CountUnpinned);

            if (count == 0)
                return;

            var offset = CountUnpinned - count;
            for (int i = 0; i < count; i++)
                Virtualize(this[CountPinnedHead + offset + i]);

            ElementCollection.RemoveRange(BlockViewStartIndex + CountPinnedHead + offset, count);
            CountUnpinned -= count;
        }

        internal void VirtualizeAll()
        {
            if (!IsInitialized)
                return;

            for (int i = 0; i < Count; i++)
                Virtualize(this[i]);
            ElementCollection.RemoveRange(BlockViewStartIndex, Count);

            _countPinnedHead = _countPinnedTail = -1;
            CountUnpinned = 0;
        }
    }
}
