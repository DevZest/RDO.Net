using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ElementManager : RowManager
    {
        private class RealizedRowCollection : IReadOnlyList<RowPresenter>
        {
            public RealizedRowCollection(ElementManager elementManager)
            {
                Debug.Assert(elementManager != null);
                _elementManager = elementManager;
            }

            private ElementManager _elementManager;
            private List<RowPresenter> _realizedRows = new List<RowPresenter>();

            private Template Template
            {
                get { return _elementManager.Template; }
            }

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
                get { return _realizedRows.Count; }
            }

            public RowPresenter this[int index]
            {
                get
                {
                    Debug.Assert(index >= 0 && index < Count);
                    return _realizedRows[index];
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
                for (int i = 0; i < Count; i++)
                {
                    var row = this[i];
                    Virtualize(row);
                }
                _realizedRows.Clear();
            }

            public void RealizeFirst(RowPresenter row)
            {
                Debug.Assert(Count == 0 && row != null && row.Ordinal >= 0);
                Realize(row);
                _realizedRows.Add(row);
            }

            public void RealizePrev()
            {
                var prevRow = _elementManager.Rows[First.Ordinal - 1];
                Realize(prevRow);
                _realizedRows.Insert(0, prevRow);
            }

            public void RealizeNext()
            {
                var nextRow = _elementManager.Rows[Last.Ordinal + 1];
                Realize(nextRow);
                _realizedRows.Add(nextRow);
            }

            public RowPresenter First
            {
                get { return Count > 0 ? this[0] : null; }
            }

            public RowPresenter Last
            {
                get { return Count > 0 ? this[Count - 1] : null; }
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

                base.EditingRow = value;
            }
        }

        private RealizedRowCollection _realizedRows;
        private FrameworkElement _elementsPanel;
        private IElementCollection _elements;
        private IElementCollection Elements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _elements;
            }
        }

        internal IReadOnlyList<UIElement> ElementList
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _elements;
            }
        }

        internal void SetElementsPanel(FrameworkElement elementsPanel)
        {
            Debug.Assert(_elementsPanel != elementsPanel);

            _elementsPanel = elementsPanel;

            if (_elements != null)
            {
                _realizedRows.VirtualizeAll();
                var scalarItems = Template.ScalarItems;
                for (int i = 0; i < scalarItems.Count; i++)
                {
                    var scalarItem = scalarItems[i];
                    var element = ElementList[i];
                    scalarItem.Cleanup(element);
                }
                _elements.Clear();
                _elements = null;
            }
        }

        private void EnsureElementCollectionInitialized()
        {
            if (_elements != null)
                return;
            _elements = IElementCollectionFactory.Create(_elementsPanel);

            var scalarItems = Template.ScalarItems;

            for (int i = 0; i < scalarItems.Count; i++)
            {
                var scalarItem = scalarItems[i];
                var element = scalarItem.Generate();
                scalarItem.Initialize(element);
                _elements.Add(element);
            }
        }

        private int _flowCount = 1;
        internal int FlowCount
        {
            get { return _flowCount; }
            set
            {
                if (_flowCount == value)
                    return;

                _flowCount = value;
            }
        }


        internal sealed override void InvalidateView()
        {
        }
    }
}
