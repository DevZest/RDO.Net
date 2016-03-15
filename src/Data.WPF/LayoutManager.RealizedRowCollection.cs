using DevZest.Data.Windows.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    partial class LayoutManager
    {
        private class RealizedRowCollection : IReadOnlyList<RowPresenter>
        {
            public RealizedRowCollection(LayoutManager layoutManager)
            {
                Debug.Assert(layoutManager != null);
                _layoutManager = layoutManager;
            }

            private LayoutManager _layoutManager;

            private Template Template
            {
                get { return _layoutManager.Template; }
            }

            private IReadOnlyList<UIElement> Elements
            {
                get { return _layoutManager.Elements; }
            }

            private IElementCollection Children
            {
                get { return _layoutManager.Children; }
            }

            public int Count
            {
                get { return Elements.Count - Template.ScalarItems.Count; }
            }

            public RowPresenter this[int index]
            {
                get
                {
                    Debug.Assert(index >= 0 && index < Count);
                    var rowView = (RowView)Elements[index + Template.ScalarItemsCountBeforeRepeat];
                    return rowView.RowPresenter;
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

            Func<RowView> _rowViewConstructor = () => new RowView();
            public void InitRowViewConstructor(Func<RowView> rowViewConstructor)
            {
                _rowViewConstructor = rowViewConstructor;
            }

            List<RowView> _cachedRowViews;
            private RowView GenerateRowView()
            {
                if (_cachedRowViews == null || _cachedRowViews.Count == 0)
                    return _rowViewConstructor();

                var last = _cachedRowViews.Count - 1;
                var result = _cachedRowViews[last];
                _cachedRowViews.RemoveAt(last);
                return result;
            }

            private void RecycleRowView(RowView rowView)
            {
                Debug.Assert(rowView != null);

                if (_cachedRowViews == null)
                    _cachedRowViews = new List<RowView>();
                _cachedRowViews.Add(rowView);
            }

            public void Add(RowPresenter row)
            {
                Debug.Assert(row != null && row.View == null);

                int index;
                if (Count == 0 || row.Index == this[0].Index - 1)
                    index = Template.ScalarItemsCountBeforeRepeat;
                else
                {
                    Debug.Assert(row.Index == this[Count - 1].Index + 1);
                    index = Template.ScalarItemsCountBeforeRepeat + Count;
                }

                var rowView = GenerateRowView();
                rowView.Initialize(row);
                row.View = rowView;
                Children.Insert(index, rowView);
            }

            public void RemoveRange(int startIndex, int count)
            {
                for (int i = startIndex; i < count; i++)
                {
                    var row = this[i];
                    var rowView = this[i].View;
                    rowView.Cleanup();
                    row.View = null;
                    RecycleRowView(rowView);
                }

                int startChildrenIndex = startIndex + Template.ScalarItemsCountBeforeRepeat;
                Children.RemoveRange(startChildrenIndex, count);
            }

            public void RemoveAll()
            {
                RemoveRange(0, Count);
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
    }
}
