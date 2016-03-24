using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ElementManager : RowManager
    {
        private interface IElementCollection : IList<UIElement>, IReadOnlyList<UIElement>
        {
            FrameworkElement Parent { get; }

            void RemoveRange(int index, int count);
        }

        private static IElementCollection CreateElementCollection(FrameworkElement parent)
        {
            if (parent == null)
                return new ElementList();
            else
                return new ChildElementCollection(parent);
        }

        private sealed class ElementList : List<UIElement>, IElementCollection
        {
            public FrameworkElement Parent
            {
                get { return null; }
            }
        }

        private sealed class ChildElementCollection : UIElementCollection, IElementCollection
        {
            public ChildElementCollection(FrameworkElement parent)
                : base(parent, parent)
            {
                Parent = parent;
            }

            public FrameworkElement Parent { get; private set; }

            public bool IsReadOnly
            {
                get { return false; }
            }

            void ICollection<UIElement>.Add(UIElement item)
            {
                base.Add(item);
            }

            IEnumerator<UIElement> IEnumerable<UIElement>.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            bool ICollection<UIElement>.Remove(UIElement item)
            {
                if (Contains(item))
                {
                    base.Remove(item);
                    return true;
                }

                return false;
            }
        }

        internal interface IRealizedRowCollection : IReadOnlyList<RowPresenter>
        {
            RowPresenter First { get; }
            RowPresenter Last { get; }
            bool Contains(RowPresenter row);
            void VirtualizeAll();
            void VirtualizeTop(int count);
            void VirtualizeBottom(int count);
            void RealizeFirst(RowPresenter row);
            void RealizePrev();
            void RealizeNext();
        }

        private class RealizedRowCollection : IRealizedRowCollection
        {
            public RealizedRowCollection(ElementManager elementManager)
            {
                Debug.Assert(elementManager != null);
                _elementManager = elementManager;
            }

            private ElementManager _elementManager;

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return _elementManager.Rows; }
            }

            private IElementCollection Elements
            {
                get { return _elementManager._elements; }
            }

            private int ScalarElementsCountBeforeRepeat
            {
                get { return _elementManager._scalarElementsCountBeforeRepeat; }
            }

            public RowPresenter First { get; private set; }

            public RowPresenter Last { get; private set; }

            public bool Contains(RowPresenter row)
            {
                Debug.Assert(row != null && row.RowManager == _elementManager);
                if (Count == 0)
                    return false;
                var ordinal = row.Ordinal;
                return ordinal >= First.Ordinal && ordinal <= Last.Ordinal;
            }

            public int Count
            {
                get { return First == null ? 0 : Last.Ordinal - First.Ordinal + 1 ; }
            }

            public RowPresenter this[int index]
            {
                get
                {
                    Debug.Assert(index >= 0 && index < Count);
                    return Rows[index + First.Ordinal];
                }
            }

            public IEnumerator<RowPresenter> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            List<RowView> _cachedRowViews;
            private RowView GetOrCreateRowView()
            {
                if (_cachedRowViews == null || _cachedRowViews.Count == 0)
                    return new RowView();

                var last = _cachedRowViews.Count - 1;
                var result = _cachedRowViews[last];
                _cachedRowViews.RemoveAt(last);
                return result;
            }

            private void Recycle(RowView rowView)
            {
                Debug.Assert(rowView != null);

                if (_cachedRowViews == null)
                    _cachedRowViews = new List<RowView>();
                _cachedRowViews.Add(rowView);
            }

            public void Realize(RowPresenter row)
            {
                Debug.Assert(row != null);

                if (row.View != null)
                    return;

                var rowView = GetOrCreateRowView();
                row.View = rowView;
            }

            public void Virtualize(RowPresenter row)
            {
                Debug.Assert(row != null && row.View != null);

                if (row.IsEditing)
                    return;

                var rowView = row.View;
                row.View = null;
                Recycle(rowView);
            }

            public void VirtualizeAll()
            {
                VirtualizeTop(Count);
            }

            public void VirtualizeTop(int count)
            {
                Debug.Assert(count >= 0 && count <= Count);

                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                    Virtualize(this[i]);

                Elements.RemoveRange(ScalarElementsCountBeforeRepeat, count);

                if (count == Count)
                    First = Last = null;
                else
                    First = Rows[First.Ordinal + count];
            }

            public void VirtualizeBottom(int count)
            {
                Debug.Assert(count >= 0 && count <= Count);

                if (count == 0)
                    return;

                var startIndex = Count - count;
                for (int i = 0; i < count; i++)
                    Virtualize(this[startIndex + i]);

                Elements.RemoveRange(ScalarElementsCountBeforeRepeat + startIndex, count);

                if (count == Count)
                    First = Last = null;
                else
                    Last = Rows[Last.Ordinal - count];
            }

            public void RealizeFirst(RowPresenter row)
            {
                Debug.Assert(Count == 0 && row != null);
                Realize(row);
                First = Last = row;

                Elements.Insert(ScalarElementsCountBeforeRepeat, row.View);
            }

            public void RealizePrev()
            {
                var prevRow = _elementManager.Rows[First.Ordinal - 1];
                Realize(prevRow);
                Elements.Insert(ScalarElementsCountBeforeRepeat, prevRow.View);
                First = prevRow;
            }

            public void RealizeNext()
            {
                var nextRow = _elementManager.Rows[Last.Ordinal + 1];
                Realize(nextRow);
                Elements.Insert(ScalarElementsCountBeforeRepeat + Count, nextRow.View);
                Last = nextRow; // This line will change the Count property, must be executed last
            }
        }

        internal ElementManager(DataSet dataSet)
            : base(dataSet)
        {
            _realizedRows = new RealizedRowCollection(this);
        }

        public sealed override RowPresenter EditingRow
        {
            internal set
            {
                var oldValue = base.EditingRow;
                if (oldValue == value)
                    return;

                if (oldValue != null && _realizedRows.Contains(oldValue))
                    _realizedRows.Virtualize(oldValue);

                if (value != null)
                    _realizedRows.Realize(value);

                base.EditingRow = value;
            }
        }

        private RealizedRowCollection _realizedRows;
        internal IRealizedRowCollection RealizedRows
        {
            get { return _realizedRows; }
        }

        private IElementCollection _elements;
        private int _scalarElementsCountBeforeRepeat;
        internal IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            Debug.Assert(_elements == null && RealizedRows.Count == 0);

            _elements = CreateElementCollection(elementsPanel);

            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
                InsertScalarElementsAfter(scalarItems[i], Elements.Count - 1, 1);
            _scalarElementsCountBeforeRepeat = Template.ScalarItemsCountBeforeRepeat;
        }

        private int InsertScalarElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = scalarItem.Generate();
                scalarItem.Initialize(element);
                _elements.Insert(index + i + 1, element);
            }
            return index + count;
        }

        private void RemoveScalarElementsAfter(ScalarItem scalarItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index];
                Debug.Assert(element.GetTemplateItem() == scalarItem);
                scalarItem.Recycle(element);
                _elements.RemoveAt(index + 1);
            }
        }

        private int _flowCount = 1;
        internal int FlowCount
        {
            get { return _flowCount; }
            set
            {
                Debug.Assert(value >= 1);

                if (_flowCount == value)
                    return;

                var delta = value - _flowCount;
                _flowCount = value;
                OnFlowCountChanged(delta);
            }
        }

        private void OnFlowCountChanged(int flowCountDelta)
        {
            Debug.Assert(flowCountDelta != 0);

            var index = -1;
            var delta = 0;
            var scalarItems = Template.ScalarItems;
            for (int i = 0; i < scalarItems.Count; i++)
            {
                index++;
                if (i == Template.ScalarItemsCountBeforeRepeat)
                    index += RealizedRows.Count;
                var scalarItem = scalarItems[i];
                if (scalarItem.RepeatMode == ScalarRepeatMode.None || scalarItem.RepeatMode == ScalarRepeatMode.Grow)
                    continue;

                if (i < Template.ScalarItemsCountBeforeRepeat)
                    delta += flowCountDelta;

                if (flowCountDelta > 0)
                    index = InsertScalarElementsAfter(scalarItem, index, flowCountDelta);
                else
                    RemoveScalarElementsAfter(scalarItem, index, -flowCountDelta);
            }

            _scalarElementsCountBeforeRepeat += delta;
        }

        internal sealed override void InvalidateView()
        {
        }
    }
}
