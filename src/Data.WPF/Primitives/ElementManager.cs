using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ElementManager : RowManager
    {
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

            private readonly ElementManager _elementManager;

            private Template Template
            {
                get { return _elementManager.Template; }
            }

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return _elementManager.Rows; }
            }

            private IElementCollection Elements
            {
                get { return _elementManager._elements; }
            }

            private int DataElementsCountBeforeRepeat
            {
                get { return _elementManager._dataElementsCountBeforeRepeat; }
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
                    return Template.RowViewConstructor();

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
                rowView.Initialize(row);
                if (Template.RowViewInitializer != null)
                    Template.RowViewInitializer(rowView);
            }

            public void Virtualize(RowPresenter row)
            {
                Debug.Assert(row != null && row.View != null);

                if (row.IsEditing)
                    return;

                var rowView = row.View;
                if (Template.RowViewCleanupAction != null)
                    Template.RowViewCleanupAction(rowView);
                row.View = null;
                rowView.Cleanup();
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

                Elements.RemoveRange(DataElementsCountBeforeRepeat, count);

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

                Elements.RemoveRange(DataElementsCountBeforeRepeat + startIndex, count);

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

                Elements.Insert(DataElementsCountBeforeRepeat, row.View);
            }

            public void RealizePrev()
            {
                var prevRow = _elementManager.Rows[First.Ordinal - 1];
                Realize(prevRow);
                Elements.Insert(DataElementsCountBeforeRepeat, prevRow.View);
                First = prevRow;
            }

            public void RealizeNext()
            {
                var nextRow = _elementManager.Rows[Last.Ordinal + 1];
                Realize(nextRow);
                Elements.Insert(DataElementsCountBeforeRepeat + Count, nextRow.View);
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
        private int _dataElementsCountBeforeRepeat;
        internal IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            Debug.Assert(_elements == null && RealizedRows.Count == 0);

            _elements = ElementCollectionFactory.Create(elementsPanel);

            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
                InsertDataElementsAfter(dataItems[i], Elements.Count - 1, 1);
            _dataElementsCountBeforeRepeat = Template.DataItemsCountBeforeRepeat;
        }

        internal void RefreshElements()
        {
            if (Elements.Count == 0)
                return;

            var index = RefreshDataElements(0, Template.DataItemsCountBeforeRepeat, 0);
            Debug.Assert(index == _dataElementsCountBeforeRepeat);

            index += RefreshRealizedRows();
            index = RefreshDataElements(Template.DataItemsCountBeforeRepeat, Template.DataItems.Count, index);
            Debug.Assert(index == Elements.Count);

            _isDirty = false;
        }

        private int RefreshDataElements(int dataItemIndex, int dataItemCount, int elementIndex)
        {
            var dataItems = Template.DataItems;
            for (int i = dataItemIndex; i < dataItemCount; i++)
            {
                var dataItem = dataItems[i];
                var crossRepeats = dataItem.CrossRepeatable ? CrossRepeats : 1;
                for (int j = 0; j < crossRepeats; j++)
                {
                    var element = Elements[elementIndex++];
                    Debug.Assert(element.GetTemplateItem() == dataItem);
                    dataItem.UpdateTarget(element);
                }
            }
            return elementIndex;
        }

        private int RefreshRealizedRows()
        {
            for (int i = 0; i < RealizedRows.Count; i++)
            {
                var row = RealizedRows[i];
                row.RefreshElements();
            }
            return RealizedRows.Count;
        }

        internal void ClearElements()
        {
            Debug.Assert(_elements != null);

            _realizedRows.VirtualizeAll();
            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
            {
                var dataItem = dataItems[i];
                dataItem.AccumulatedCrossRepeatsDelta = 0;
                int count = dataItem.CrossRepeatable ? CrossRepeats : 1;
                RemoveDataElementsAfter(dataItem, -1, count);
            }
            Debug.Assert(Elements.Count == 0);
            _crossRepeats = 1;
            _elements = null;
        }

        private int InsertDataElementsAfter(DataItem dataItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = dataItem.Generate();
                _elements.Insert(index + i + 1, element);
                dataItem.Initialize(element);
            }
            return index + count;
        }

        private void RemoveDataElementsAfter(DataItem dataItem, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var element = Elements[index + 1];
                Debug.Assert(element.GetTemplateItem() == dataItem);
                dataItem.Cleanup(element);
                _elements.RemoveAt(index + 1);
            }
        }

        private int _crossRepeats = 1;
        internal int CrossRepeats
        {
            get { return _crossRepeats; }
            set
            {
                Debug.Assert(value >= 1);

                if (_crossRepeats == value)
                    return;

                var delta = value - _crossRepeats;
                _crossRepeats = value;
                OnCrossRepeatsChanged(delta);
            }
        }

        private void OnCrossRepeatsChanged(int crossRepeatsDelta)
        {
            Debug.Assert(crossRepeatsDelta != 0);

            var index = -1;
            var delta = 0;
            var dataItems = Template.DataItems;
            for (int i = 0; i < dataItems.Count; i++)
            {
                index++;
                if (i == Template.DataItemsCountBeforeRepeat)
                    index += RealizedRows.Count;
                var dataItem = dataItems[i];

                var prevAccumulatedCrossRepeatsDelta = i == 0 ? 0 : dataItems[i - 1].AccumulatedCrossRepeatsDelta;
                if (!dataItem.CrossRepeatable)
                {
                    dataItem.AccumulatedCrossRepeatsDelta = prevAccumulatedCrossRepeatsDelta + (CrossRepeats - 1);
                    continue;
                }
                dataItem.AccumulatedCrossRepeatsDelta = prevAccumulatedCrossRepeatsDelta;

                if (i < Template.DataItemsCountBeforeRepeat)
                    delta += crossRepeatsDelta;

                if (crossRepeatsDelta > 0)
                    index = InsertDataElementsAfter(dataItem, index, crossRepeatsDelta);
                else
                    RemoveDataElementsAfter(dataItem, index, -crossRepeatsDelta);
            }

            _dataElementsCountBeforeRepeat += delta;
        }

        private bool _isDirty;
        internal sealed override void Invalidate(RowPresenter row)
        {
            if (_isDirty || _elements == null)
                return;

            if (row == null || (RealizedRows.Contains(row) && row.Elements != null))
            {
                _isDirty = true;
                BeginRefreshElements();
            }
        }

        private void BeginRefreshElements()
        {
            Debug.Assert(_elements != null && _isDirty);

            var panel = _elements.Parent;
            if (panel == null)
                RefreshElements();
            else
            {
                panel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshElements();
                }));
            }
        }
    }
}
