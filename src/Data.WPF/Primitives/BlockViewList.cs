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

            private int StartIndex
            {
                get { return _elementManager.BlockViewListStartIndex; }
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
                    return (BlockView)Elements[StartIndex + index];
                }
            }

            public override void RealizeFirst(int blockOrdinal)
            {
                Debug.Assert(Count == 0 && blockOrdinal >= 0 && blockOrdinal < MaxCount);
                _elementManager.Realize(blockOrdinal);
                _count += 1;
            }

            public override void RealizePrev()
            {
                Debug.Assert(First != null && First.Ordinal >= 1);
                _elementManager.Realize(First.Ordinal - 1);
                _count += 1;
            }

            public override void RealizeNext()
            {
                Debug.Assert(Last != null && Last.Ordinal + 1 < MaxCount);
                _elementManager.Realize(Last.Ordinal + 1);
                _count += 1;
            }

            public override void VirtualizeAll()
            {
                if (_count == 0)
                    return;

                _elementManager.VirtualizeAll();
                _count = 0;
            }
        }

        public abstract BlockView this[int index] { get; }

        public bool Contains(RowView rowView)
        {
            return this[rowView] != null;
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

        public BlockView this[RowView rowView]
        {
            get
            {
                if (Count == 0)
                    return null;

                var index = IndexOf(rowView.BlockOrdinal);
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

        public abstract void RealizeFirst(int blockOrdinal);

        public abstract void RealizePrev();

        public abstract void RealizeNext();

        public abstract void VirtualizeAll();

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
