using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ContainerViewList : IReadOnlyList<ContainerView>
    {
        public static ContainerViewList Empty
        {
            get { return EmptyContainerViewList.Singleton; }
        }

        public static ContainerViewList Create(ElementManager elementManager)
        {
            return new BlockViewListImpl(elementManager);
        }

        private sealed class EmptyContainerViewList : ContainerViewList
        {
            public static readonly EmptyContainerViewList Singleton = new EmptyContainerViewList();

            private EmptyContainerViewList()
            {
            }

            public override ContainerView this[int index]
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

        private sealed class BlockViewListImpl : ContainerViewList
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

            public override ContainerView this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return (ContainerView)Elements[StartIndex + index];
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
                Debug.Assert(First != null && First.ContainerOrdinal >= 1);
                _elementManager.Realize(First.ContainerOrdinal - 1);
                _count += 1;
            }

            public override void RealizeNext()
            {
                Debug.Assert(Last != null && Last.ContainerOrdinal + 1 < MaxCount);
                _elementManager.Realize(Last.ContainerOrdinal + 1);
                _count += 1;
            }

            public override void VirtualizeAll()
            {
                if (_count == 0)
                    return;

                _elementManager.VirtualizeBlockViewList();
                _count = 0;
            }
        }

        public abstract ContainerView this[int index] { get; }

        public bool Contains(RowView rowView)
        {
            return this[rowView] != null;
        }

        public abstract int Count { get; }

        public IEnumerator<ContainerView> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int MaxCount { get; }

        public ContainerView this[RowView rowView]
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

            if (ordinal >= first.ContainerOrdinal && ordinal <= Last.ContainerOrdinal)
                return ordinal - first.ContainerOrdinal;
            return -1;
        }

        public ContainerView First
        {
            get { return Count == 0 ? null : this[0]; }
        }

        public ContainerView Last
        {
            get { return Count == 0 ? null : this[Count - 1]; }
        }

        public abstract void RealizeFirst(int containerOrdinal);

        public abstract void RealizePrev();

        public abstract void RealizeNext();

        public abstract void VirtualizeAll();

        private bool IsRealized(int ordinal)
        {
            return First != null && ordinal >= First.ContainerOrdinal && ordinal <= Last.ContainerOrdinal;
        }

        public ContainerView GetContainerView(int ordinal)
        {
            return IsRealized(ordinal) ? this[ordinal - First.ContainerOrdinal] : null;
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
