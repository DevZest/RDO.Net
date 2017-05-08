using DevZest.Windows.Controls.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Windows.Data.Primitives
{
    public abstract class ContainerViewList : IReadOnlyList<ContainerView>
    {
        internal static ContainerViewList Empty
        {
            get { return EmptyContainerViewList.Singleton; }
        }

        internal static ContainerViewList Create(ElementManager elementManager)
        {
            return new ConcreteContainerViewList(elementManager);
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

            internal override void RealizeFirst(int blockOrdinal)
            {
                throw new NotSupportedException();
            }

            internal override void RealizeNext()
            {
                throw new NotSupportedException();
            }

            internal override void RealizePrev()
            {
                throw new NotSupportedException();
            }

            internal override void VirtualizeAll()
            {
            }

            internal override void VirtualizeFirst()
            {
                throw new NotSupportedException();
            }

            internal override void VirtualizeLast()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class ConcreteContainerViewList : ContainerViewList
        {
            internal ConcreteContainerViewList(ElementManager elementManager)
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
                get { return _elementManager.ContainerViewListStartIndex; }
            }

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return _elementManager.Rows; }
            }

            private int FlowCount
            {
                get { return _elementManager.FlowCount; }
            }

            public override int MaxCount
            {
                get { return Rows.Count == 0 ? 0 : (Rows.Count - 1) / FlowCount + 1; }
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

            internal override void RealizeFirst(int blockOrdinal)
            {
                Debug.Assert(Count == 0 && blockOrdinal >= 0 && blockOrdinal < MaxCount);
                _elementManager.Realize(blockOrdinal);
                _count += 1;
            }

            internal override void RealizePrev()
            {
                Debug.Assert(First != null && First.ContainerOrdinal >= 1);
                _elementManager.Realize(First.ContainerOrdinal - 1);
                _count += 1;
            }

            internal override void RealizeNext()
            {
                Debug.Assert(Last != null && Last.ContainerOrdinal + 1 < MaxCount);
                _elementManager.Realize(Last.ContainerOrdinal + 1);
                _count += 1;
            }

            internal override void VirtualizeAll()
            {
                if (_count == 0)
                    return;

                _elementManager.VirtualizeAll();
                _count = 0;
            }

            internal override void VirtualizeFirst()
            {
                Debug.Assert(First != null);

                _elementManager.VirtualizeFirst();
                _count--;
            }

            internal override void VirtualizeLast()
            {
                Debug.Assert(Last != null);

                _elementManager.VirtualizeLast();
                _count--;
            }
        }

        public abstract ContainerView this[int index] { get; }

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
        public int IndexOf(int ordinal)
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

        internal abstract void RealizeFirst(int containerOrdinal);

        internal abstract void RealizePrev();

        internal abstract void RealizeNext();

        internal abstract void VirtualizeAll();

        internal abstract void VirtualizeFirst();

        internal abstract void VirtualizeLast();

        private bool IsRealized(int ordinal)
        {
            return First != null && ordinal >= First.ContainerOrdinal && ordinal <= Last.ContainerOrdinal;
        }

        public ContainerView GetContainerView(int ordinal)
        {
            return IsRealized(ordinal) ? this[ordinal - First.ContainerOrdinal] : null;
        }
    }
}
