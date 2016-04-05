using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal class StackViewCollection : IReadOnlyList<StackView>
    {
        internal StackViewCollection(ElementManager elementManager)
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

        private int StackViewStartIndex
        {
            get { return _elementManager.StackViewStartIndex; }
        }

        List<StackView> _cachedStackViews;

        private StackView Realize(int stackIndex)
        {
            var stackView = CachedList.GetOrCreate(ref _cachedStackViews, Template.StackViewConstructor);
            stackView.Initialize(_elementManager, stackIndex);
            return stackView;
        }

        private void Virtualize(StackView stackView)
        {
            Debug.Assert(stackView != null);

            if (Template.StackViewCleanupAction != null)
                Template.StackViewCleanupAction(stackView);
            stackView.Cleanup();
            CachedList.Recycle(ref _cachedStackViews, stackView);
        }

        public StackView First
        {
            get { return Count == 0 ? null : (StackView)Elements[StackViewStartIndex]; }
        }

        public StackView Last
        {
            get { return Count == 0 ? null : (StackView)Elements[StackViewStartIndex + Count - 1]; }
        }

        private IReadOnlyList<RowPresenter> Rows
        {
            get { return _elementManager.Rows; }
        }

        private int StackDimensions
        {
            get { return _elementManager.StackDimensions; }
        }

        public RowPresenter FirstRow
        {
            get { return Count == 0 ? null : Rows[First.Index * StackDimensions]; }
        }

        public RowPresenter LastRow
        {
            get { return Count == 0 ? null : Rows[Math.Min(Rows.Count - 1, (Last.Index + 1) * StackDimensions - 1)]; }
        }

        public bool Contains(RowPresenter row)
        {
            Debug.Assert(row != null && row.RowManager == _elementManager);
            if (Count == 0)
                return false;
            return row.Ordinal >= FirstRow.Ordinal && row.Ordinal <= LastRow.Ordinal;
        }

        public int Count { get; private set; }

        public StackView this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < Count);
                return (StackView)Elements[StackViewStartIndex + index];
            }
        }

        public IEnumerator<StackView> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void RealizeFirst(int index)
        {
            Debug.Assert(Count == 0 && index >= 0 && index * StackDimensions < Rows.Count);

            var stackView = Realize(index);
            ElementCollection.Insert(StackViewStartIndex, stackView);
            Count = 1;
        }

        internal void RealizePrev()
        {
            Debug.Assert(First != null && First.Index > 0);

            var stackView = Realize(First.Index - 1);
            ElementCollection.Insert(StackViewStartIndex, stackView);
            Count++;
        }

        internal void RealizeNext()
        {
            Debug.Assert(LastRow != null && LastRow.Ordinal < Rows.Count - 1);

            var stackView = Realize(Last.Index + 1);
            ElementCollection.Insert(StackViewStartIndex + Count, stackView);
            Count++;
        }

        internal void VirtualizeHead(int count)
        {
            Debug.Assert(count >= 0 && count <= Count);

            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
                Virtualize(this[i]);

            ElementCollection.RemoveRange(StackViewStartIndex, count);
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

            ElementCollection.RemoveRange(StackViewStartIndex + offset, count);
            Count -= count;
        }

        internal void VirtualizeAll()
        {
            VirtualizeHead(Count);
        }
    }
}
