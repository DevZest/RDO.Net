using DevZest.Data.Windows.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class BlockViewList : IReadOnlyList<BlockView>
    {
        public static BlockViewList Empty
        {
            get { return EmptyBlockViewList.Singleton; }
        }

        public static BlockViewList Create(ElementManager elementManager)
        {
            return new BlockViewListImpl(elementManager);
        }

        private sealed class EmptyBlockViewList : BlockViewList
        {
            public static readonly EmptyBlockViewList Singleton = new EmptyBlockViewList();

            private EmptyBlockViewList()
            {
            }

            public override BlockView this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public override int Count
            {
                get { return 0; }
            }

            public override int MaxCount
            {
                get { throw new NotSupportedException(); }
            }

            public override void Clear()
            {
            }

            public override void RealizeFirst(int blockOrdinal)
            {
                throw new NotSupportedException();
            }

            public override void RealizeNext()
            {
                throw new NotSupportedException();
            }

            public override void RealizePrev()
            {
                throw new NotSupportedException();
            }

            public override void VirtualizeAll()
            {
                throw new NotSupportedException();
            }

            public override void VirtualizeHead(int count)
            {
                throw new NotSupportedException();
            }

            public override void VirtualizeTail(int count)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class BlockViewListImpl : BlockViewList
        {
            internal BlockViewListImpl(ElementManager elementManager)
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
                get { return _elementManager.HeadScalarElementsCount; }
            }

            List<BlockView> _cachedBlockViews;

            private BlockView Realize(int blockOrdinal)
            {
                var blockView = CachedList.GetOrCreate(ref _cachedBlockViews, Template.BlockViewConstructor);
                blockView.Initialize(_elementManager, blockOrdinal);
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

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return _elementManager.Rows; }
            }

            private int BlockDimensions
            {
                get { return _elementManager.BlockDimensions; }
            }

            public override int MaxCount
            {
                get { return Rows.Count == 0 ? 0 : (Rows.Count - 1) / BlockDimensions + 1; }
            }

            private int _count;
            public override int Count
            {
                get { return _count; }
            }

            public override BlockView this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return (BlockView)Elements[BlockViewStartIndex + index];
                }
            }

            private void Insert(int index, BlockView blockView)
            {
                ElementCollection.Insert(index, blockView);
                Template.InitializeBlockView(blockView);
            }

            public override void RealizeFirst(int blockOrdinal)
            {
                Debug.Assert(Count == 0 && blockOrdinal >= 0 && blockOrdinal < MaxCount);

                var blockView = Realize(blockOrdinal);
                Insert(BlockViewStartIndex, blockView);
                _count = 1;
            }

            public override void RealizePrev()
            {
                Debug.Assert(First != null && First.Ordinal >= 1);

                var blockView = Realize(First.Ordinal - 1);
                Insert(BlockViewStartIndex, blockView);
                _count++;
            }

            public override void RealizeNext()
            {
                Debug.Assert(Last != null && Last.Ordinal + 1 < MaxCount);

                var blockView = Realize(Last.Ordinal + 1);
                Insert(BlockViewStartIndex + Count, blockView);
                _count++;
            }

            public override void VirtualizeHead(int count)
            {
                Debug.Assert(count >= 0 && count <= Count);

                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                    Virtualize(this[i]);

                ElementCollection.RemoveRange(BlockViewStartIndex, count);
                _count -= count;
            }

            public override void VirtualizeTail(int count)
            {
                Debug.Assert(count >= 0 && count <= Count);

                if (count == 0)
                    return;

                var offset = Count - count;
                for (int i = 0; i < count; i++)
                    Virtualize(this[offset + i]);

                ElementCollection.RemoveRange(BlockViewStartIndex + offset, count);
                _count -= count;
            }

            public override void VirtualizeAll()
            {
                for (int i = 0; i < Count; i++)
                    Virtualize(this[i]);
                ElementCollection.RemoveRange(BlockViewStartIndex, Count);
                _count = 0;
            }

            public override void Clear()
            {
                for (int i = 0; i < Count; i++)
                    Virtualize(this[i]);
                ElementCollection.RemoveRange(BlockViewStartIndex, Count);
                _count = 0;
            }
        }

        public abstract BlockView this[int index] { get; }

        public bool Contains(RowPresenter row)
        {
            return this[row] != null;
        }

        public abstract int Count { get; }

        public IEnumerator<BlockView> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int MaxCount { get; }

        public BlockView this[RowPresenter row]
        {
            get
            {
                if (Count == 0)
                    return null;

                var index = IndexOf(row.BlockOrdinal);
                return index == -1 ? null : this[index];
            }
        }

        private int IndexOf(int ordinal)
        {
            Debug.Assert(ordinal >= 0 && ordinal < MaxCount);

            var first = First;
            if (first == null)
                return -1;

            if (ordinal >= first.Ordinal && ordinal <= Last.Ordinal)
                return ordinal - first.Ordinal;
            return -1;
        }

        public BlockView First
        {
            get { return Count == 0 ? null : this[0]; }
        }

        public BlockView Last
        {
            get { return Count == 0 ? null : this[Count - 1]; }
        }

        public abstract void VirtualizeHead(int count);

        public abstract void VirtualizeTail(int count);

        public abstract void VirtualizeAll();

        public abstract void RealizeFirst(int blockOrdinal);

        public abstract void RealizePrev();

        public abstract void RealizeNext();

        public abstract void Clear();

        private bool IsRealized(int blockOrdinal)
        {
            return First != null && blockOrdinal >= First.Ordinal && blockOrdinal <= Last.Ordinal;
        }

        public BlockView GetBlockView(int blockOrdinal)
        {
            return IsRealized(blockOrdinal) ? this[blockOrdinal - First.Ordinal] : null;
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
